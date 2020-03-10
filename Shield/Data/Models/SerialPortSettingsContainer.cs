using Shield.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    [DataContract(Name = "SerialPortSettingsContainer")]
    public class SerialPortSettingsContainer : ISerialPortSettingsContainer
    {
        [DataMember]
        private List<ISerialPortSettingsModel> _adapters = new List<ISerialPortSettingsModel>();

        public SettingsType Type { get; }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) => SetDefaults();

        public void SetDefaults()
        {
            if (_adapters?.Count > 0)
                foreach (var a in _adapters)
                    a.SetDefaults();
        }

        // TODO replace simple number with name method (in serial port adapter i think)
        public void Add(ISerialPortSettingsModel settings)
        {
            if (_adapters.Any(s => s.PortNumber == settings.PortNumber))
                return;
            _adapters.Add(settings);
        }

        public ISerialPortSettingsModel GetSettingsByPortNumber(int number)
        {
            return _adapters.Where(a => a.PortNumber == number).FirstOrDefault();
        }

        public bool RemoveByPortNumber(int number)
        {
            return _adapters.Remove(_adapters.Find(a => a.PortNumber == number));
        }
    }
}