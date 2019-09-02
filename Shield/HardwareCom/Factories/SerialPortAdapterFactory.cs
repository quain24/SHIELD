using Shield.Data.Models;
using Shield.HardwareCom.Adapters;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace Shield.HardwareCom.Factories
{
    public class SerialPortAdapterFactory : ISerialPortAdapterFactory
    {
        private SerialPortAdapter _portAdapter;
        private SerialPort _port;

        public bool Create(ISerialPortSettingsModel settings)
        {
            return Create(settings.PortNumber,
                          settings.CommandSize,
                          settings.BaudRate,
                          settings.DataBits,
                          settings.Parity,
                          settings.StopBits);
        }

        public bool Create(int portNumber,
                           int commandSize,
                           int baudRate,
                           int dataBits,
                           Parity parity,
                           StopBits stopBits)
        {
            string portName = "COM" + portNumber;

            if (!AvailablePorts.Contains(portName))
            {
                return false;
            }

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

            _portAdapter = new SerialPortAdapter(_port, commandSize);

            return true;
        }

        public SerialPortAdapter GivePort => _portAdapter;

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
            _portAdapter = null;
        }
    }
}
