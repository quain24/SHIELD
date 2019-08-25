using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.Enums;
using System.Diagnostics;

namespace Shield.HardwareCom
{
    public class ComSender : IComSender
    {
        private SerialPort _port;
        private ICommand _command;
        public void Setup(SerialPort port)
        {
            _port = port;
        }

        public void Message(ICommand command)
        {
            _command = command;
        }        

        public bool Send()
        {
            if (_port == null || _command == null)
                return false;

            if (!_port.IsOpen)
                _port.Open();

            try
            {
                if (_command.CommandType == CommandType.Data)
                    _port.Write(_command.Data);
                else
                    _port.Write(_command.CommandTypeString);

                return true;
            }
            catch(Exception ex)
            {
                if(ex is TimeoutException)
                {
                    Debug.WriteLine("Timeout exception: " + ex.Message);
                }
                else
                {
                    Debug.WriteLine("Other exception: " + ex.Message);                   
                }
                return false;
            }
        }

    }
}
