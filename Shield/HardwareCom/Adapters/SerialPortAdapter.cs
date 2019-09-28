using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.Extensions;
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
    // Przeanalizować tą klasę, popoprawiać i zoptymalizować co jest, upewnić się, że nie ma potrzeby niczego dodatkowego i zakończyć!

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

        private readonly SerialPort _port;
        private IAppSettings _appSettings;

        private StringBuilder _receivedBuffer = new StringBuilder();
        private int _dataSize;
        private int _idSize;
        private int _commandTypeSize;
        private int _completeCommandSizeWithSep;
        private bool _isLissening = false;
        private Regex CommandPattern;
        ISerialPortSettingsModel _internalSettings;

        public event EventHandler<string> DataReceived;

        #endregion Internal variables and events

        public SerialPortAdapter(SerialPort port, IAppSettings appSettings)
        {
            _port = port;
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

            _internalSettings = (ISerialPortSettingsModel)settings;

            if (!SerialPort.GetPortNames().Contains($"COM{_internalSettings.PortNumber}"))
                return false;

            _port.PortName = $"COM{_internalSettings.PortNumber}";
            _port.BaudRate = _internalSettings.BaudRate;
            _port.DataBits = _internalSettings.DataBits;
            _port.Parity = _internalSettings.Parity;
            _port.StopBits = _internalSettings.StopBits;
            _port.ReadTimeout = _internalSettings.ReadTimeout;
            _port.WriteTimeout = _internalSettings.WriteTimeout;
            _port.Encoding = Encoding.GetEncoding(_internalSettings.Encoding);

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
        /// Constantly monitors incoming communication data, filters it and generates Commands (ICommandModel)
        /// </summary>
        public async Task StartReceivingAsync()
        {
            _isLissening = true;
            string errorData = string.Empty;
            byte[] mainBuffer = new byte[_completeCommandSizeWithSep * 10]; // times ten gives sufficient overhead on high speed transmissions

            while (_port.IsOpen && !_receiveToken.IsCancellationRequested)
            {
                int bytesRead = await _port.BaseStream.ReadAsync(mainBuffer, 0, mainBuffer.Length, _receiveToken);
                string rawData = Encoding.GetEncoding(_port.Encoding.CodePage).GetString(mainBuffer).Substring(0, bytesRead);

                if (_port.Encoding.CodePage == Encoding.ASCII.CodePage)
                    _receivedBuffer.Append(rawData.RemoveASCIIChars());
                else
                    _receivedBuffer.Append(rawData);

                if (_receivedBuffer.Length >= _completeCommandSizeWithSep)
                {
                    string workPiece = _receivedBuffer.ToString(0, _completeCommandSizeWithSep);
                    int whereToCut = CheckRawData(workPiece);

                    if (whereToCut > 0)
                    {
                        _receivedBuffer.Remove(0, whereToCut);
                        workPiece = workPiece.Substring(0, whereToCut);
                    }

                    // All good or all bad
                    else
                        _receivedBuffer.Remove(0, _completeCommandSizeWithSep);

                    DataReceived?.Invoke(this, workPiece);
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
            _receiveTokenSource = new CancellationTokenSource();
            _receiveToken = _receiveTokenSource.Token;
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>Task<bool> if sends or failes</bool></returns>
        public async Task<bool> SendAsync(string command)
        {
            if (string.IsNullOrEmpty(command) || _sendToken.IsCancellationRequested)
                return false;

            byte[] sendBuffer = Encoding.GetEncoding(_port.Encoding.CodePage).GetBytes(command);
            try
            {
                await _port.BaseStream.WriteAsync(sendBuffer, 0, sendBuffer.Count(), _sendToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"ERROR - SerialPortAdapter - SendAsync: one or more Commands could not be sent - port closed / unavailible / cancelled?");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>bool if sends or failes</bool></returns>
        public bool Send(string command)
        {
            if (command is null)
                return false;

            string raw = command;

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
                return firsIndexOfSepprarator;
        }

        #endregion Helpers - raw data checking and transformation
    }
}