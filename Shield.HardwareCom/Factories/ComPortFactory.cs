using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows;

namespace Shield.HardwareCom.Factories
{
    public class ComPortFactory : IComPortFactory
    {
        SerialPort _port;
        public bool Create(int portNumber,
                           int baudRate = 19200,
                           int dataBits = 8,
                           Parity parity = Parity.None,
                           StopBits stopBits = StopBits.One)
        {
            string portName = "COM" + portNumber;
            
            if (!AvailablePorts.Contains(portName))
                return false;

            _port = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                Encoding = Encoding.ASCII
            };

            return true;
        }

        public SerialPort GivePort
        {
            get
            {
                return _port;
            }
        }
        public List<string> AvailablePorts
        {
            get
            {
                var list = new List<string>();
                foreach (string s in SerialPort.GetPortNames())
                {
                    list.Add(s);
                }
                return list;
            }
        }

        private void Clear()
        {
            _port = null;
        }


    }
}
