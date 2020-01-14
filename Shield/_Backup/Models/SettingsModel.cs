using Shield.CommonInterfaces;
using Shield.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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