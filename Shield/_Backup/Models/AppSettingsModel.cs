using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Shield.Data.Models
{
    /// <summary>
    /// Contains all of the available settings, for serialization and deserialization. Is used by AppSettings object to give and store settings
    /// not to be used alone
    /// </summary>

    // When adding new type to serialization, insert another knowntype!
    [KnownType(typeof(SerialPortSettingsModel))]
    [KnownType(typeof(MoqPortSettingsModel))]
    [KnownType(typeof(ApplicationSettingsModel))]
    [KnownType(typeof(CommandTypesModel))]
    [DataContract(Name = "Configuration", Namespace = "ShieldAppSettings")]
    [XmlRoot("ApplicationSettings")]
    public class AppSettingsModel : IAppSettingsModel
    {
        [DataMember(Name = "Settings")]
        private Dictionary<SettingsType, ISetting> _settings = new Dictionary<SettingsType, ISetting>();

        public Dictionary<SettingsType, ISetting> Settings
        {
            get => _settings;
            set => _settings = value;
        }
    }
}