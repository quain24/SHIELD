using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{    
    public class ComReceiver
    {
        private SerialPort _port;

        public void Setup(SerialPort port)
        {
            _port = port;
        }

        public void Receive()
        {

        }

        public async Task ReceiveAsync()
        {
           
        }
    }
}