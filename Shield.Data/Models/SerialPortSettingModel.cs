using Shield.CommonInterfaces;
using Shield.Enums;
using System.IO.Ports;
using System.Runtime.Serialization;

namespace Shield.Persistance.Models
{
    [DataContract(Name = "SerialPortSettings")]
    public class SerialPortSettingsModel  /*ISerialPortSettingsModel*/
        // TODO Apply repository pattern for things like settings etc. Each thing should have separate repository - settings, saved commands etc
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
        public int ReadTimeout { get; set; }

        [DataMember]
        public int WriteTimeout { get; set; }

        [DataMember]
        public int Encoding { get; set; }

        /// <summary>
        /// how long should application wait for message confirmation (in milliseconds)
        /// </summary>
        [DataMember]
        public int CompletitionTimeout { get; set; }

        /// <summary>
        /// How long a message should be held before it is marked as error because of being incomplete (in milliseconds).
        /// </summary>
        [DataMember]
        public int ConfirmationTimeout { get; set; }

        public string Name { get => $"COM{PortNumber}"; private set { } }

        private SettingsType _type;

        public SettingsType Type { get => _type; }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => SetDefaults();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) => Name = $"COM{PortNumber}";

        public void SetDefaults()
        {
            PortNumber = 5;
            BaudRate = 19200;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            ReadTimeout = -1;
            ReadTimeout = -1;
            WriteTimeout = -1;
            Encoding = System.Text.Encoding.ASCII.CodePage;
            CompletitionTimeout = 0;
            ConfirmationTimeout = 0;

            _type = SettingsType.SerialDevice;
        }
    }
}