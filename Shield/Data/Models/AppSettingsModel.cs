using Shield.CommonInterfaces;
using Shield.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Shield.Data.Models
{
    // When adding new type to serialization, insert aother knowntype!
    [KnownType(typeof(SerialPortSettingsModel))]
    [KnownType(typeof(MoqPortSettingsModel))]
    [KnownType(typeof(ApplicationSettingsModel))]

    [DataContract(Name = "Configuration", Namespace = "ShieldAppSettings")]
    [XmlRoot("ApplicationSettings")]

    public class AppSettingsModel : IAppSettingsModel
    {
        [XmlElement("Settings")]
        [DataMember(Name = "Settings")]
        private Dictionary<SettingsType, ISettings> _settings = new Dictionary<SettingsType, ISettings>();

        public Dictionary<SettingsType, ISettings> Settings
        {
            get => _settings;
            set => _settings = value;
        }
    }
}
