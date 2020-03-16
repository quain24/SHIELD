using Shield.CommonInterfaces;
using Shield.Enums;

namespace Shield.Data.Models
{
    public interface ICommunicationDeviceSettingsContainer : ISetting
    {
        void Add(ICommunicationDeviceSettings settings);
        ICommunicationDeviceSettings GetSettingsByDeviceName(string name);
        bool RemoveByDeviceName(string name);
    }
}