using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Shield.Persistence.Models
{
    [DataContract(Name = "CommunicationDeviceSettingsContainer")]
    public class CommunicationDeviceSettingsContainer : ICommunicationDeviceSettingsContainer
    {
        [DataMember]
        private readonly List<ICommunicationDeviceSettings> _adapters = new List<ICommunicationDeviceSettings>();

        public SettingsType Type { get; private set; }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => SetDefaults();

        public void SetDefaults() => Type = SettingsType.CommunicationDeviceSettingsContainer;

        public void Add(ICommunicationDeviceSettings settings)
        {
            _ = settings ?? throw new System.ArgumentNullException(nameof(settings));

            if (_adapters.Any(s => s.Name == settings.Name))
                throw new System.Exception($@"Device ""{settings.Name}"" already exists in this container");
            _adapters.Add(settings);
        }

        public ICommunicationDeviceSettings GetSettingsByDeviceName(string name) => _adapters.Find(a => a.Name == name);

        public bool RemoveByDeviceName(string name) => _adapters.Remove(_adapters.Find(a => a.Name == name));
    }
}