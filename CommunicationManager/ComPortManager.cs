using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using Autofac;

namespace CommunicationManager
{
    public class ComPortManager : IComPortManager
    {        
        private Func<SerialPort> _portFactory;
        private SerialPort _port;

        public ComPortManager(Func<SerialPort> portFactory)
        {
            _portFactory = portFactory;             // umozliwia tworzenie nowych instancji obiektu serialport
        }

        public void Create(int portNumber = 5,
                               int baudRate = 19200,
                               int dataBits = 8,
                               Parity parity = Parity.None,
                               StopBits stopBits = StopBits.One)
        {
            Clear();
            _port = _portFactory.Invoke();

            _port.PortName = "COM" + portNumber.ToString();
            _port.BaudRate = baudRate;
            _port.DataBits = dataBits;
            _port.Parity = parity;
            _port.StopBits = stopBits;
            _port.ReadTimeout = 1000;
            _port.WriteTimeout = 1000;
            _port.Encoding = Encoding.BigEndianUnicode;        
        }

        public int ReadTimeout
        {
            get { return this._port.ReadTimeout; }
            set { _port.ReadTimeout = value > 0 ? value : 1000; }
        }

        public int WriteTimeout
        {
            get { return _port.WriteTimeout; }
            set { _port.WriteTimeout = value > 0 ? value : 1000; }
        }

        public Encoding Encoding
        {
            get { return _port.Encoding; }
            set { _port.Encoding = value; }
        }

        public SerialPort GetPort()
        {
            return _port;
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

        public void Clear()
        {
            _port = null;            
        }       
    }
}
