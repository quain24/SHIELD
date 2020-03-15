using Shield.Enums;
using System.Runtime.Serialization;

namespace Shield.Data.Models
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

        public MoqPortSettingsModel()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            PortNumber = 2;
            _type = SettingsType.MoqDevice;
            CompletitionTimeout = 3000;
            ConfirmationTimeout = 4000;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }
    }
}