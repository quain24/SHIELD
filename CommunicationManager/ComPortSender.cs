using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using CommunicationManager.Models;

namespace CommunicationManager
{
    public class ComPortSender
    {
        public ComPortSender()
        {
        }

        public ComPortSender(SerialPort port)
        {          
           
        }
        public ComPortSender(SerialPort port, IMessageModel message)
        {

        }
    }
}
