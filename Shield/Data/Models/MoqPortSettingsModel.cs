using System;

namespace Shield.Data.Models
{
    [Serializable]
    public class MoqPortSettingsModel : IMoqPortSettingsModel
    {
        public int PortNumber { get; set; } = 8;
    }
}