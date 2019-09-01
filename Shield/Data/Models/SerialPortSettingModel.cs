using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Shield.Enums;

namespace Shield.Data.Models
{
    [Serializable]
    public class SerialPortSettingsModel : ISerialPortSettingsModel
    {
        public int PortNumber { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public DeviceType DeviceType { get; set; }
    }
}
