using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    [DataContract(Name = "ApplicationSettings")]
    public class SettingsModel : ISettingsModel
    {
        private Dictionary<SettingsType, ISetting> _settings = new Dictionary<SettingsType, ISetting>();

        [DataMember(Name = "Settings")]
        public Dictionary<SettingsType, ISetting> Settings
        {
            get => _settings;
            set => _settings = value;
        }
    }
}