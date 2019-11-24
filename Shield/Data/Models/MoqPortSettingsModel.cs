using Shield.Enums;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    [DataContract(Name = "MoqPortSettings")]
    public class MoqPortSettingsModel : IMoqPortSettingsModel
    {
        [DataMember]
        public int PortNumber { get; set; }

        public SettingsType Type { get; set; }

        public MoqPortSettingsModel()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            PortNumber = 2;
            Type = SettingsType.MoqDevice;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }
    }
}