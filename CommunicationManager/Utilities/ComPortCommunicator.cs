using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace CommunicationManager.Utilities
{
    public class ComPortCommunicator : IComPortCommunicator, IDisposable
    {
        private SerialPort _serialPort = new SerialPort();
        private SerialPort _serialReceiver = new SerialPort();
        public ComPortCommunicator()
        {
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.PortName = "COM5";
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Encoding = Encoding.BigEndianUnicode;

            _serialReceiver.ReadTimeout = 1000;
            _serialReceiver.WriteTimeout = 1000;
            _serialReceiver.PortName = "COM6";
            _serialReceiver.BaudRate = 9600;
            _serialReceiver.DataBits = 8;
            _serialReceiver.Parity = Parity.None;
            _serialReceiver.StopBits = StopBits.One;
            _serialReceiver.Encoding = Encoding.BigEndianUnicode;

            _serialReceiver.DataReceived += SerialPortDataReceived;
            _serialReceiver.Open();
            _serialPort.Open();
        }

        
        public SerialPort GiveReceiver()
        {
            return this._serialReceiver;
        }
        public void SendData(string data)
        {            
            _serialPort.Write(data);
        }

        public void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.WriteLine("Data Received:");
            Console.WriteLine(indata);
        }

        public void Dispose()
        {
            _serialReceiver.Close();
            _serialPort.Close();
            _serialReceiver.DataReceived -= SerialPortDataReceived;
        }
    }
}
