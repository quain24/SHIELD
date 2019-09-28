using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Shield.Enums;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    [Serializable]
    public class SerialPortSettingsModel : ISerialPortSettingsModel
    {
        public int PortNumber { get; set; } = 5;
        public int BaudRate { get; set; } = 19200;
        public int DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;
        public DeviceType DeviceType { get; set; } = DeviceType.Serial;
        public int ReadTimeout { get; set; } = -1;
        public int WriteTimeout { get; set; } = -1;
        public int Encoding { get; set; } = System.Text.Encoding.ASCII.CodePage;
    }
}
