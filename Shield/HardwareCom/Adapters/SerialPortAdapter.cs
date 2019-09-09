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

        private const char SEPARATOR = '*';
        private const char FILLER = '*';

        private CancellationTokenSource ts = new CancellationTokenSource();   
        private CancellationToken ct;

        private object _lock = new object();
        private object _lockStringBuilder = new object();

        private readonly SerialPort _port;
        private int _commandSize;
        private int _receiverInterval;
        private StringBuilder _receivedBuffer = new StringBuilder { Length = 0 };
        private Func<ICommandModel> _commandModelFac;        
        private IAppSettings _appSettings;

        public event EventHandler<ICommandModel> DataReceived;

        public SerialPortAdapter (SerialPort port, Func<ICommandModel> commandModelFac, IAppSettings appSettings)
        {
            _port = port;
            _commandModelFac = commandModelFac;
            _appSettings = appSettings;
        }        

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
        
        private void StartReceiving()
        {
            ct = ts.Token;
            Task.Run(() => ConstantReceiverAsync(), ct);
        }

        // Do malego dopracowania / poprawek
        private async Task ConstantReceiverAsync()
        {
            while (_port.IsOpen)
            {               
                if (ct.IsCancellationRequested)
                {        
                    Debug.WriteLine("constant receiver cancelled");
                    break;
                }
                
                if(_port.BytesToRead == 0 && _receivedBuffer.Length < _commandSize)
                {
                    await Task.Delay(_receiverInterval).ConfigureAwait(false);
                    continue;
                }

                _receivedBuffer.Append(_port.ReadExisting());
            
                if(_receivedBuffer.Length >= _commandSize)
                {
                    lock (_lockStringBuilder)
                    {                        
                        int check = CheckRawData(_receivedBuffer.ToString(0, _commandSize));
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
                    }

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
            if(data.IndexOf(SEPARATOR, 1) < 5)
                return  data.IndexOf(SEPARATOR, 1);

            if(data[0] == SEPARATOR)
                return 0;

            return data.IndexOf(SEPARATOR);           
        }      

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
                    StartReceiving();                  
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

        public void DiscardInBuffer()
        {
            _port.DiscardInBuffer();
            _receivedBuffer.Clear().Length = 0;
        }        

        // Do sprawdzenia, do zobaczenia jak to złączyć z messangerem
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

        public void Send(ICommandModel command)
        {
            // opracować co jeżeli nic nie zostanie zapisane - handle exceptions!
            // przerobienie na typ string, wybór co ma wysłać i jak - tutaj czy w messanger?
            // poprawić - na razie po macoszemu napisane
            // co jezeli nie ma dokad wyslac - port odbiorczy jest zamkniety - handle exce

            // czy tutaj ma byc wysylanie w osobnym watku, czy w miejscu wywołania????
            
           // Task.Run(() =>{


            if(command is null)
                return;
            bool err = false;
            int tmpRawCommandType = (int) command.CommandType;
            string rawCommandType = tmpRawCommandType.ToString();
            if(rawCommandType.Length > 4 || rawCommandType.Length == 0)
                err = true;
            else
            {            
                rawCommandType = rawCommandType.PadLeft(4, '0');
                rawCommandType = SEPARATOR + rawCommandType + SEPARATOR;
            }

            if(!err)
                rawCommandType += command.Data;
            _port.Write(rawCommandType);
        //});
        }
        
        

        public void Dispose()
        {   
            ts.Cancel();
            _port.Close();
        }
    }
}
