using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Shield.Data.Models
{
    /// <summary>
    /// Contains all of the availible settings, for serialization and deserialization. Is used by AppSettings object to give and store settings
    /// not to be used alone
    /// </summary>

    // When adding new type to serialization, insert another knowntype!
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