using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.CommonInterfaces;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Adapters
{
    public class SerialPortAdapter : ICommunicationDevice
    {
        private  readonly SerialPort _port;
        
        public SerialPortAdapter(SerialPort port)
        {
            _port = port;
            _port.DataReceived += PropagateDataReceivedEvent;
        }        

        public event EventHandler DataReceived;

        private void PropagateDataReceivedEvent(object sender, EventArgs e)
        {
            this.DataReceived?.Invoke(sender, e);
        }        
        
        public void Open()
        {
            if (_port != null && !_port.IsOpen)
            {
                _port.Open();
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
                    Debug.WriteLine("Port was not open! " + e.Message);
                    throw e;
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
        }        

        public string Read()
        {
            return _port.ReadExisting();
        }

        public void Write(string rawData)
        {
            // opracować co jeżeli nic nie zostanie zapisane - handle exceptions!
            _port.Write(rawData);
        }

        

        public void Dispose()
        {   
            _port.DataReceived -= PropagateDataReceivedEvent;
            _port.Close();
        }
    }
}
