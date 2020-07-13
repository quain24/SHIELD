using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Shield.Persistence.Models
{
    [DataContract(Name = "ApplicationSettings")]
    public class SettingsModel : ISettingsModel
    {
        [DataMember(Name = "Settings")]
        public Dictionary<SettingsType, ISetting> Settings { get; set; } = new Dictionary<SettingsType, ISetting>();
    }

    // TODO rethink settings interfaces - device settings etc. Where to put them,
    // should operate on different model than is used to save them?
    // remove concrete device interface for settings and replace them with general IDeviceSettings?
}