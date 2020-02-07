using Shield.Enums;
using System.IO.Ports;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    [DataContract(Name = "SerialPortSettings")]
    public class SerialPortSettingsModel : ISerialPortSettingsModel
    {
        public SerialPortSettingsModel()
        {
            SetDefaults();
        }

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

        private SettingsType _type;

        public SettingsType Type { get => _type; }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }

        public void SetDefaults()
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
            _type = SettingsType.SerialDevice;
        }
    }
}