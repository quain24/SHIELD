using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Adapters
{
    // Ogolnie przemyslec protokol - czy w rozmiarze uwzgledniamy separatory????
    // skoro dodano id, to pomyslec nad korekcja bledow

    public class SerialPortAdapter : ICommunicationDevice
    {
        /// <summary>
        /// Wraps SerialPort for future use with interfaces
        /// Works on commandMNodel's, sends and receives them with translation from and into them
        /// </summary>

        #region Internal variables and events

        private const char SEPARATOR = '*';
        private const char FILLER = '.';

        private CancellationTokenSource _receiveTokenSource = new CancellationTokenSource();
        private CancellationToken _receiveToken;
        private CancellationTokenSource _sendTokenSource = new CancellationTokenSource();
        private CancellationToken _sendToken;

        private object _lock = new object();
        private object _lockStringBuilder = new object();
        private object _lockSender = new object();

        private readonly SerialPort _port;
        private Func<ICommandModel> _commandModelFac;
        private IAppSettings _appSettings;

        private StringBuilder _receivedBuffer = new StringBuilder();
        private int _dataSize;
        private int _idSize;
        private int _commandTypeSize;
        private int _completeCommandSizeWithSep;
        private bool _isLissening = false;
        private Regex CommandPattern;

        public event EventHandler<ICommandModel> DataReceived;

        #endregion Internal variables and events

        public SerialPortAdapter(SerialPort port, Func<ICommandModel> commandModelFac, IAppSettings appSettings)
        {
            _port = port;
            _commandModelFac = commandModelFac;
            _appSettings = appSettings;            
        }

        /// <summary>
        /// MANDATORY setup, without it this instance will not work.
        /// </summary>
        /// <param name="settings">Configuration from AppSettings for this type of device</param>
        /// <returns>True if successfull</returns>
        public bool Setup(ICommunicationDeviceSettings settings)
        {
            if (_port is null || settings is null || !(settings is ISerialPortSettingsModel))
                return false;

            ISerialPortSettingsModel internalSettings = (ISerialPortSettingsModel)settings;

            if (!SerialPort.GetPortNames().Contains($"COM{internalSettings.PortNumber}"))
                return false;

            _port.PortName = $"COM{internalSettings.PortNumber}";
            _port.BaudRate = internalSettings.BaudRate;
            _port.DataBits = internalSettings.DataBits;
            _port.Parity = internalSettings.Parity;
            _port.StopBits = internalSettings.StopBits;
            _port.ReadTimeout = internalSettings.ReadTimeout;
            _port.WriteTimeout = internalSettings.WriteTimeout;
            _port.Encoding = Encoding.GetEncoding(internalSettings.Encoding);

            _port.DtrEnable = false;
            _port.RtsEnable = false;
            _port.DiscardNull = true;

            IApplicationSettingsModel appSett = (IApplicationSettingsModel)_appSettings.GetSettingsFor(SettingsType.Application);

            // Size of command combines message size, id size and 1 separator after those
            _idSize = appSett.IdSize;
            _dataSize = appSett.DataSize;
            _commandTypeSize = appSett.CommandTypeSize;
            _completeCommandSizeWithSep = _commandTypeSize + 2 + _idSize + 1 + _dataSize;

            _sendToken = _sendTokenSource.Token;
            _receiveToken = _receiveTokenSource.Token;

            string pattern = $@"[{SEPARATOR}][0-9]{{{_commandTypeSize}}}[{SEPARATOR}][a-zA-Z0-9]{{{_idSize}}}[{SEPARATOR}]";
            CommandPattern = new Regex(pattern);
            
            return true;
        }

        /// <summary>
        /// Constantly monitors incoming communication data, filters it and generates Commands (ICommandModel)
        /// </summary>
        public async Task StartReceivingAsync()
        {
            _isLissening = true;
            string errorData = string.Empty;
            byte[] mainBuffer = new byte[_completeCommandSizeWithSep];

            while (_port.IsOpen && !_receiveToken.IsCancellationRequested)
            {
                int bytesRead = await _port.BaseStream.ReadAsync(mainBuffer, 0, _completeCommandSizeWithSep, _receiveToken);
                string rawData = Encoding.GetEncoding(_port.Encoding.CodePage).GetString(mainBuffer).Substring(0, bytesRead);

                if (_port.Encoding.CodePage == Encoding.ASCII.CodePage)
                    _receivedBuffer.Append(RemoveNonAsciiChars(rawData));
                else
                    _receivedBuffer.Append(rawData);

                if (_receivedBuffer.Length >= _completeCommandSizeWithSep)
                {
                    ICommandModel command = _commandModelFac();

                    string workPiece = _receivedBuffer.ToString(0, _completeCommandSizeWithSep);
                    int whereToCut = CheckRawData(workPiece);

                    if (whereToCut == -1)
                    {
                        command.Data = workPiece;
                        _receivedBuffer.Remove(0, _completeCommandSizeWithSep);
                    }
                    else if (whereToCut > 0)
                    {
                        command.Data = _receivedBuffer.ToString(0, whereToCut);
                        _receivedBuffer.Remove(0, whereToCut);
                    }

                    if (whereToCut != 0)
                        command.CommandType = CommandType.Error;
                    else
                    {
                        command = CommandTranslator(workPiece);
                        _receivedBuffer.Remove(0, _completeCommandSizeWithSep);
                    }
                    DataReceived?.Invoke(this, command);
                }
                else
                {
                    continue;
                }
            }
            _isLissening = false;
        }

        public void StopReceiving()
        {
            _receiveTokenSource.Cancel();
        }

        public void Open()
        {
            lock (_lock)
            {
                if (_port != null && !_port.IsOpen)
                {
                    _port.Open();
                }
            }
        }

        public void Close()
        {
            StopReceiving();
            // Close the serial port in a new thread to avoid freezes
            Task closeTask = new Task(() =>
            {
                try
                {
                    _port.Close();
                }
                catch (IOException e)
                {
                    // Port was not open
                    Debug.WriteLine("MESSAGE: SerialPortAdapter Close - Port was not open! " + e.Message);
                }
            });
            closeTask.Start();

            //return closeTask;

            // odniorca:
            // await serialStream.Close();
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>Task<bool> if sends or failes</bool></returns>
        public async Task<bool> SendAsync(ICommandModel command)
        {
            bool result = await Task.Run((Func<bool>)(() =>
          {
              if (command is null)
                  return false;

              string raw = CommandTranslator(command);

              if (!string.IsNullOrEmpty(raw) && !_sendToken.IsCancellationRequested)
              {
                  try
                  {
                      _port.Write(raw);
                      return true;
                  }
                  catch (Exception ex)
                  {
                      Debug.WriteLine($@"ERROR - SerialPortAdapter - SendAsync: one or more Commands could not be sent - port closed / unavailible?");
                      return false;
                  }
              }
              return false;
          }), _sendToken);

            return result;
        }

        public bool Send(ICommandModel command)
        {
            if (command is null)
                return false;

            string raw = CommandTranslator(command);

            if (!string.IsNullOrEmpty(raw))
            {
                try
                {
                    _port.Write(raw);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($@"ERROR - SeiralPortAdapter - Send: could not send a command - port closed / unavailible?");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Clears data in 'received' buffer
        /// </summary>
        public void DiscardInBuffer()
        {
            try
            {
                _port.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"ERROR - SerialPortAdaper - DiscardBuffer: Port was closed, nothing to discard.");
            }

            lock (_lock)
            {
                _receivedBuffer.Clear().Length = 0;
            }
        }

        public void Dispose()
        {
            _receiveTokenSource.Cancel();
            DiscardInBuffer();
            _port.Close();

            // Added for port to close in peace, otherwise there could be a problem with opening it again.
            Thread.Sleep(100);
        }

        #region Helpers - raw data checking and transformation

        /// <summary>
        /// Internal check of incoming data.
        /// <para>takes raw data and returns either '0' if correct, '-1' if compleatly bad or '(int) index to which to remove if another try could be succesfull</para>
        /// </summary>
        /// <param name="data">Input string - typically a Command-sized part of received data</param>
        /// <returns>Index to start a correction from, '-1' if data is all wrong or 0 when input is in correct format</returns>
        private int CheckRawData(string data)
        {
            Match match = CommandPattern.Match(data);
            if (match.Success)
                return match.Index;

            int firsIndexOfSepprarator = data.IndexOf(SEPARATOR);

            if (firsIndexOfSepprarator == -1)
                return firsIndexOfSepprarator;

            if (firsIndexOfSepprarator == 0)
                return 1;
            else
            {
                return firsIndexOfSepprarator;
            }
        }

        private string RemoveNonAsciiChars(string data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in data)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == SEPARATOR || c == FILLER)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Takes a preformatted string of raw data and translates it to a single CommandModel
        /// </summary>
        /// <param name="rawData">preformatted string, typically from received buffer</param>
        /// <returns>Single command in CommandModel format</returns>
        private ICommandModel CommandTranslator(string rawData)
        {
            ICommandModel command = _commandModelFac();
            string rawCom;
            string rawDat = string.Empty;
            string rawId = string.Empty;

            int commandSizeWithSepparators = _commandTypeSize + 2;
            int idWithSepparator = _idSize + 1;

            if (rawData.Length >= _commandTypeSize + 2)
            {
                rawCom = rawData.Substring(0, commandSizeWithSepparators);    // Command in *0123* format (including asterisc or other SEPARATOR)
                rawDat = rawData.Substring(commandSizeWithSepparators + idWithSepparator); // Data starts after command type and id. Example: *0001*A8DD*12345678912345
                rawId = rawData.Substring(commandSizeWithSepparators, idWithSepparator); // Get it with sepparator

                if (rawCom.First() == SEPARATOR &&
                    rawCom.Last() == SEPARATOR)
                {
                    int rawComInt;
                    if (Int32.TryParse(rawCom.Substring(1, _commandTypeSize), out rawComInt))
                    {
                        if (Enum.IsDefined(typeof(CommandType), rawComInt))
                            command.CommandType = (CommandType)rawComInt;
                        else
                            command.CommandType = CommandType.Unknown;
                    }
                }
            }

            // If command is still empty, then raw data was wrong - device cannot send empty, useless communication.
            if (command.CommandType == CommandType.Empty)
                command.CommandType = CommandType.Error;

            command.Id = rawId.Substring(0, _idSize);
            command.Data = rawDat;

            return command;
        }

        /// <summary>
        /// Translates a CommandModel into a raw formatted string if given a correct command or returns empty string for error
        /// </summary>
        /// <param name="givenCommand">Command to be trasformed into raw string</param>
        /// <returns>Raw formatted string that can be understood by connected machine</returns>
        private string CommandTranslator(ICommandModel givenCommand)
        {
            if (givenCommand is null || !Enum.IsDefined(typeof(CommandType), givenCommand.CommandType))
                return null;

            StringBuilder command = new StringBuilder(SEPARATOR.ToString());

            command.Append(givenCommand.CommandType.ToString().PadLeft(_commandTypeSize, '0')).Append(SEPARATOR);
            command.Append(givenCommand.Id).Append(SEPARATOR);

            if (givenCommand.CommandType == CommandType.Data)
                command.Append(givenCommand.Data);

            if (command.Length < _completeCommandSizeWithSep)
                command.Append(FILLER, _completeCommandSizeWithSep - command.Length);

            return command.ToString();
        }

        #endregion Helpers - raw data checking and transformation
    }
}