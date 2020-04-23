namespace Shield.CommonInterfaces
{
    public interface ICommunicationDeviceSettingsContainer : ISetting
    {
        void Add(ICommunicationDeviceSettings settings);

        ICommunicationDeviceSettings GetSettingsByDeviceName(string name);

        bool RemoveByDeviceName(string name);
    }
}