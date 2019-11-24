using Shield.Enums;
using System;
using System.IO.Ports;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    [DataContract(Name = "SerialPortSettings")]
    public class SerialPortSettingsModel : ISerialPortSettingsModel
    {
        [DataMember]
        public int PortNumber { get; set; }

        [DataMember]
        public int BaudRate { get; set; }

        [DataMember]
        public int DataBits { get; set; }

        [DataMember]
        public Parity Parity { get; set; }

        [DataMember]
        public StopBits StopBits { get; set; }

        [DataMember]
        public DeviceType DeviceType { get; set; }

        [DataMember]
        public int ReadTimeout { get; set; }

        [DataMember]
        public int WriteTimeout { get; set; }

        [DataMember]
        public int Encoding { get; set; }

        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {            
            PortNumber = 5;
            BaudRate = 19200;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            DeviceType = DeviceType.Serial;
            ReadTimeout = -1;
            ReadTimeout = -1;
            WriteTimeout = -1;
            Encoding = System.Text.Encoding.ASCII.CodePage;
        }
    }
}