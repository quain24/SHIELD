using System;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{    
    [DataContract(Name = "MoqPortSettings")]
    public class MoqPortSettingsModel : IMoqPortSettingsModel
    {        
        [DataMember]
        public int PortNumber { get; set; }

        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            PortNumber = 2;
        }
    }
}