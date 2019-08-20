using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace CommunicationManager
{
    public class ComPortManager : IComPortManager
    {
        SerialPort port;

        public void Create(int portNumber = 5,
                               int baudRate = 19200,
                               int dataBits = 8,
                               Parity parity = Parity.None,
                               StopBits stopBits = StopBits.One)
        {

            port = new SerialPort
            {
                PortName = "COM" + portNumber.ToString(),
                BaudRate = baudRate,
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                Encoding = Encoding.BigEndianUnicode
            };
        }

        public int ReadTimeout
        {
            get { return this.port.ReadTimeout; }
            set { port.ReadTimeout = value > 0 ? value : 1000; }
        }

        public int WriteTimeout
        {
            get { return port.WriteTimeout; }
            set { port.WriteTimeout = value > 0 ? value : 1000; }
        }

        public Encoding Encoding
        {
            get { return port.Encoding; }
            set { port.Encoding = value; }
        }

        public SerialPort GetPort()
        {
            return port;
        }

        public void Clear()
        {
            port = null;
        }
    }
}
