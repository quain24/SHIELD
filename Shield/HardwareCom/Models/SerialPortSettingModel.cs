using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Shield.Enums;

namespace Shield.HardwareCom.Models
{
    [Serializable]
    public class SerialPortSettingsModel : ISerialPortSettingsModel
    {
        public int portNumber { get; set; }
        public int baudRate { get; set; }
        public int DataBits { get; set; }
        public Parity parity { get; set; }
        public StopBits stopBits { get; set; }
    }
}
