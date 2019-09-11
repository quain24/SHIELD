using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shield.CommonInterfaces;
using Shield.HardwareCom.Models;
using Shield.Enums;
using Shield.Data.Models;
using Shield.Data;
using System.Threading;

namespace Shield.HardwareCom.Adapters
{
    // Sporo zmian - do zbadania po powrocie:
    // -- przekazywanie eventu do messangera, tak samo danych do niego
    // -- wywala exception timeout przy receive - dlaczego tak długo odbiera prosty ciag znakow?

    // Czy osobny receive jest potrzebny?
    // constantreceiver po otwarciu portu moze dzialac lepiej, w koncu otwierasz, by nadawac i nasluchiwac ciagle.



    public class SerialPortAdapter : ICommunicationDevice
    {
        /// <summary>
        /// Wraps SerialPort for future use with interfaces
        /// Works on commandMNodel's, sends and receives them with translation from and into them
        /// </summary>           

        #region Internal variables and events

        private const char SEPARATOR = '*';
        private const char FILLER = '*';

        private CancellationTokenSource ts = new CancellationTokenSource();   
        private CancellationToken ct;

        private object _lock = new object();
        private object _lockStringBuilder = new object();
        private object _lockSender = new object();

        private readonly SerialPort _port;
        private int _commandSize;
        private int _receiverInterval;
        private StringBuilder _receivedBuffer = new StringBuilder { Length = 0 };
        private Func<ICommandModel> _commandModelFac;        
        private IAppSettings _appSettings;

        public event EventHandler<ICommandModel> DataReceived;

        #endregion

        public SerialPortAdapter (SerialPort port, Func<ICommandModel> commandModelFac, IAppSettings appSettings)
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
            if(_port is null || !(settings is ISerialPortSettingsModel) || settings is null)
                return false;

            ISerialPortSettingsModel internalSettings = (ISerialPortSettingsModel) settings;

            if(!SerialPort.GetPortNames().Contains($"COM{internalSettings.PortNumber}"))
                return false;

            _port.PortName = $"COM{internalSettings.PortNumber}";
            _port.BaudRate = internalSettings.BaudRate;
            _port.DataBits = internalSettings.DataBits;
            _port.Parity = internalSettings.Parity;
            _port.StopBits = internalSettings.StopBits;
            _port.ReadTimeout = internalSettings.ReadTimeout;
            _port.WriteTimeout = internalSettings.WriteTimeout;
            _port.Encoding = Encoding.GetEncoding(internalSettings.Encoding);
            
            _receiverInterval = (internalSettings.ReadTimeout / 10) <= 0 ? _receiverInterval = 0 : _receiverInterval = (internalSettings.ReadTimeout / 10);

            IApplicationSettingsModel appSett = (IApplicationSettingsModel) _appSettings.GetSettingsFor(SettingsType.Application);

            _commandSize = appSett.MessageSize;

            return true;
        }
        
        public ICommandModel Receive()
        {
            //Czy to w ogole bedzie potrzebne, skoro nasluch jest caly czas?

            // Działa lepiej jako task, czyli zapycha watek?
            // jak to zmienic? Czy trzeba, jeżeli będzie działało z messangera - tam nowy watek?
            // TODO
          try{
                Task.Run(() => Console.WriteLine(_port.ReadExisting()));
            }
            catch
            {
                Debug.WriteLine("ERROR: SerialPortAdapter Receive - receive time limit reached");
            }


            return new CommandModel {CommandType = Enums.CommandType.Data, Data = "Test data readed from SerialPortAdapter" };  // do testow, imoplementacja czeka!
        }



        /// <summary>
        /// Starts a receiving task in the background to constantly monitor incoming data.
        /// </summary>
        public async Task StartReceivingAsync()
        {
            ct = ts.Token;
            //try
            //{
                await Task.Run(() => ConstantReceiver(ct), ct);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(@"INFO - SerialPortAdapter - StartReceiving: Thread of ConstantReceiverAsync was cancelled");
            //}
        }

        /// <summary>
        /// Designed to be put in a seprate Task, constantly reading bytes from serial port,
        /// initially correcting data if needed and outputting a CommandModel when raw command is received.
        /// </summary>
        /// <returns></returns>
        private void ConstantReceiver(CancellationToken ct)
        {
            int loopCount = 0;
            while (_port.IsOpen)
            {               
                if (ct.IsCancellationRequested)
                {        
                    Debug.WriteLine("constant receiver cancelled");
                    break;
                }
                
                if(_port.BytesToRead == 0 && _receivedBuffer.Length < _commandSize)
                {
                    loopCount++;
                    if(loopCount >= 1000)
                    //await Task.Delay(_receiverInterval, ct);
                        Thread.Sleep(/*_receiverInterval*/1000);
                    //Console.WriteLine("waiting...");
                    continue;
                }

                //lock (_lockStringBuilder)
                //{                    
                    _receivedBuffer.Append(_port.ReadExisting());
                //}

                if (_receivedBuffer.Length >= _commandSize)
                {
                    //lock (_lockStringBuilder)
                    //{
                        string partOfBuffer = _receivedBuffer.ToString(0, _commandSize);
                        partOfBuffer = RemoveNonAsciiChars(partOfBuffer);

                        if(partOfBuffer.Length < _commandSize)
                        {
                            _receivedBuffer.Remove(0, _commandSize);
                            _receivedBuffer.Insert(0, partOfBuffer);
                            continue;
                        }
                        //_receivedBuffer = RemoveNonAsciiChars(partOfBuffer);
                        //if (_receivedBuffer.Length < _commandSize)
                        //    continue;

                        int check = CheckRawData(partOfBuffer);
                        if(check == -1)
                        {
                            _receivedBuffer.Remove(0, _commandSize);
                            continue;
                        }
                        else if(check > 0)
                        {
                            _receivedBuffer.Remove(0, check);
                            continue;
                        }
                    //}

                    ICommandModel command = CommandTranslator(_receivedBuffer.ToString(0, _commandSize));
                    _receivedBuffer.Remove(0, _commandSize);
                    
                    DataReceived?.Invoke(this, command);                    
                }
            }
        }

        /// <summary>
        /// Internal check of incoming data.
        /// <para>takes raw data and returns either '0' if correct, '-1' if compleatly bad or '(int) index to which to remove' if another try could be succesfull</para> 
        /// </summary>
        /// <param name="data">Input string - typically a Command-sized part of received data</param>
        /// <returns>Index to start a correction of received buffer or 0 when input is in correct format</returns>
        private int CheckRawData(string data)
        {     
            // do zoptymalizowania? pewnie tak...

            int commandTypeStart = data.IndexOf(SEPARATOR, 0);
            int commandTypeEnd = -1;

            if(commandTypeStart != -1 && commandTypeStart < _commandSize - 6)
            {
                commandTypeEnd = data.IndexOf(SEPARATOR, commandTypeStart + 1);

                if(commandTypeEnd != -1)
                {
                    if(commandTypeStart == 0 && commandTypeEnd == 5)
                        return 0;

                    else if(commandTypeStart == 0)
                        return 1;
                    else
                        return commandTypeStart;
                }
            }

            return commandTypeStart;          
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
            string rawCom = string.Empty;
            string rawDat = string.Empty;

            if(rawData.Length >= 6)
            {
                rawCom = rawData.Substring(0, 6);    // Command in *0123* format (including asterisc or other SEPARATOR)
                rawDat = rawData.Substring(6);       // Data, lenght of _commandSize minus 6 (command type header) as asdffasdf... and so on            
            
                if (rawCom.First() == SEPARATOR &&
                    rawCom.Last() == SEPARATOR)  
                {
                    int rawComInt;
                    if(Int32.TryParse(rawCom.Substring(1, 4), out rawComInt))
                    {               
                        if(Enum.IsDefined(typeof(CommandType), rawComInt))                
                            command.CommandType = (CommandType) rawComInt;                    
                    }
                }
            }

            // If command is still empty, then raw data was wrong - device cannot send empty, useless communication.
            if(command.CommandType == CommandType.Empty)
                command.CommandType = CommandType.Error;

            if(command.CommandType != CommandType.Error || command.CommandType != CommandType.Empty)
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
            if(givenCommand is null || !Enum.IsDefined(typeof(CommandType), givenCommand.CommandType))
                return string.Empty;

            StringBuilder command = new StringBuilder(SEPARATOR.ToString());

            command.Append(givenCommand.CommandType.ToString().PadLeft(4, '0')).Append(SEPARATOR);

            if(givenCommand.CommandType == CommandType.Data)
                command.Append(givenCommand.Data);
            
            if(command.Length < _commandSize)
                command.Append(FILLER, _commandSize - command.Length);

            return command.ToString();
        }
        
        // pomyslec nad tym
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
            ts.Cancel();
            // Close the serial port in a new thread
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

        // Do sprawdzenia, do zobaczenia jak to złączyć z messangerem
      
        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>Task<bool> if sends or failes</bool></returns>
        public async Task<bool> SendAsync(ICommandModel command)
        {
            bool result = await Task.Run((Func<bool>) ( () => 
            {
                if(command is null)
                    return false;
                
                string raw = CommandTranslator(command);

                if(!string.IsNullOrEmpty(raw))
                {
                    try
                    {                    
                        _port.Write(raw);
                        return true; 
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($@"ERROR - SerialPortAdapter - SendAsync: one or more Commands could not be sent - port closed / unavailible?");
                        return false;
                    }                                       
                }
                return false;
            }), ct);

            return result;
        }

        public bool Send(ICommandModel command)
        {
            if(command is null)
                return false;

            string raw = CommandTranslator(command);

            if(!string.IsNullOrEmpty(raw))
            {
                try
                {
                    _port.Write(raw);
                    return true;
                }
                catch(Exception ex)
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
                Debug.WriteLine(@"ERROR - SerialPortAdaper - DiscardBuffer: Port was allready closed, nothing to discard.");
            }
            
            _receivedBuffer.Clear().Length = 0;
        }        

        public void Dispose()
        {   
            ts.Cancel();
            DiscardInBuffer();
            _port.Close();

            // Added for port to close in peace, otherwise there could be a problem with opening it again.
            Thread.Sleep(100);
        }

        public Task StartReceiving()
        {
            throw new NotImplementedException();
        }


        // do przemianowania i bedzie używane jako główna i wlasciwie jedyna metoda działania - inne są niemiarodajne, niepewne.
        // Do optymalizacji, do sprawdzenia czy nie blokuje, do dalszych testow, do wklepania w moqadapter
        public async Task<int> ReadUsingStream()
        {
            StringBuilder internalBuffer = new StringBuilder();

            while(_port.IsOpen && !ct.IsCancellationRequested)
            {
                byte[] mainBuffer = new byte[_commandSize];
                int bytesRead = await _port.BaseStream.ReadAsync(mainBuffer, 0, _commandSize, ct);
                ICommandModel command = _commandModelFac();

                string rawData = Encoding.GetEncoding(_port.Encoding.CodePage).GetString(mainBuffer);

                internalBuffer.Append(RemoveNonAsciiChars(rawData));                

                if(internalBuffer.Length >= _commandSize)
                {
                    string workPiece = internalBuffer.ToString(0, _commandSize);
                    int whereToCut = CheckRawData(workPiece);

                    if(whereToCut == -1)
                    {
                        internalBuffer.Remove(0, _commandSize);
                        continue;
                    }
                        
                    else if(whereToCut > 0)
                    {
                        internalBuffer.Remove(0, whereToCut);
                        continue;
                    }
                        
                    else
                    {
                        command = CommandTranslator(workPiece);
                        internalBuffer.Remove(0, _commandSize);
                        DataReceived?.Invoke(this, command);
                    }                    
                }
                else
                {
                    continue;
                }




                //command = CommandTranslator(Encoding.ASCII.GetString(buffer));
                //DataReceived?.Invoke(this, command); 
                
                
            }
            return 1;            
        }






    }
}
