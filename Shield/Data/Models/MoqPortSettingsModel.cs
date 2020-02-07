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

        public MoqPortSettingsModel()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            PortNumber = 2;
            _type = SettingsType.MoqDevice;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }
    }
}