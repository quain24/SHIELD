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

namespace Shield.HardwareCom.Adapters
{
    // Sporo zmian - do zbadania po powrocie:
    // -- przekazywanie eventu do messangera, tak samo danych do niego
    // -- wywala exception timeout przy receive - dlaczego tak długo odbiera prosty ciag znakow?

    public class SerialPortAdapter : ICommunicationDevice
    {
        /// <summary>
        /// Wraps SerialPort for future use with interfaces
        /// </summary>
        private readonly SerialPort _port;
        private readonly int _commandSize;
        private StringBuilder _receivedBuffer = new StringBuilder { Length = 0 };
        private Func<ICommandModel> _commandModelFac;

        public event EventHandler<ICommandModel> DataReceived;
        
        public SerialPortAdapter(SerialPort port, int commandSize, Func<ICommandModel> commandModelFac)
        {
            _port = port;
            _commandSize = commandSize; 
            _commandModelFac = commandModelFac;
        }             

        // zmiana - wewnatrz tego obiektu bedzie przetwarzany sygnal odioru danych,
        // nastepnie on wywola 
        // sprawdzic, czy appendowanie nie zapycha nam tu bufora odbiorczego??

        private void InternalDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _receivedBuffer.Append(_port.ReadExisting());
            
            if(_receivedBuffer.Length >= _commandSize)
            {                
                ICommandModel command = CommandTranslator(_receivedBuffer.ToString(0, _commandSize));
                DataReceived?.Invoke(this, command);
            }
        }

        private ICommandModel CommandTranslator(string rawData)
        {           
            ICommandModel command = _commandModelFac();
            string rawCom = string.Empty;
            string rawDat = string.Empty;

            if(rawData.Length >= 6)
            {
                rawCom = rawData.Substring(0, 6);    // Command in *0123* format (including asterisc)
                rawDat = rawData.Substring(6);       // Data, lenght of _commandSize minus 6 (command type header) as asdffasdf... and so on            
            
                if (rawCom.First() == '*' &&
                    rawCom.Last() == '*')  
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

            if(command.CommandType != CommandType.Error)
                command.Data = rawDat;

            return command;            
        }
        private void PropagateDataReceivedEvent(object sender, ICommandModel e)
        {
            this.DataReceived?.Invoke(sender, e);
        }        
        
        public void Open()
        {
            if (_port != null && !_port.IsOpen)
            {
                _port.Open();
                _port.DataReceived += InternalDataReceived;
            }
        }
        public void Close()
        {
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
            bool err = false;
            int tmpRawCommandType = (int) command.CommandType;
            string rawCommandType = tmpRawCommandType.ToString();
            if(rawCommandType.Length > 4 || rawCommandType.Length == 0)
                err = true;
            else
            {            
                rawCommandType = rawCommandType.PadLeft(4, '0');
                rawCommandType = '*' + rawCommandType + '*';
            }

            if(!err)
                rawCommandType += command.Data;
            Open();
            _port.Write(rawCommandType);

        }

        

        public void Dispose()
        {   
            _port.DataReceived -= InternalDataReceived;
            _port.Close();
        }
    }
}
