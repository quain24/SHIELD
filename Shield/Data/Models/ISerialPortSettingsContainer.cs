using Shield.CommonInterfaces;
using Shield.Enums;

namespace Shield.Data.Models
{
    public interface ISerialPortSettingsContainer : ISetting
    {
        SettingsType Type { get; }

        void Add(ISerialPortSettingsModel settings);
        ISerialPortSettingsModel GetSettingsByPortNumber(int number);
        bool RemoveByPortNumber(int number);
    }
}