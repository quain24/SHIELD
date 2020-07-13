using Shield.CommonInterfaces;
using Shield.Enums;
using System.Runtime.Serialization;

namespace Shield.Persistence.Models
{
    [DataContract(Name = "MoqPortSettings")]
    public class MoqPortSettingsModel : IMoqPortSettingsModel
    {
        [DataMember]
        public int PortNumber { get; set; }

        private SettingsType _type;

        public SettingsType Type { get => _type; }
        public int CompletitionTimeout { get; set; }
        public int ConfirmationTimeout { get; set; }
        public string Name { get; private set; }

        public MoqPortSettingsModel() => SetDefaults();

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => SetDefaults();

        public void SetDefaults()
        {
            PortNumber = 2;
            _type = SettingsType.MoqDevice;
            CompletitionTimeout = 3000;
            ConfirmationTimeout = 4000;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) => Name = $"MOQ{PortNumber}";
    }
}