using Shield.CommonInterfaces;
using Shield.Enums;
using Shield.Persistance.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Shield.HardwareCom.Factories
{
    /// <summary>
    /// Gives a chosen type of communication device based on DeviceType provided
    /// Takes optional parameters or a configuration directly from AppSettings (loaded from file)
    /// </summary>

    public class CommunicationDeviceFactory : ICommunicationDeviceFactory
    {
        private readonly ISettings _settings;
        private readonly IReadOnlyDictionary<DeviceType, ICommunicationDevice> _deviceFactory;

        public CommunicationDeviceFactory(IReadOnlyDictionary<DeviceType, ICommunicationDevice> deviceFactory,
                                          ISettings settings)
        {
            _settings = settings;
            _deviceFactory = deviceFactory;
        }

        public ICommunicationDevice CreateDevice(ICommunicationDeviceSettings settings)
        {
            ICommunicationDevice device;
            switch (settings)
            {
                case var _ when settings is SerialPortSettingsModel:
                    device = _deviceFactory[DeviceType.Serial];
                    break;

                case var _ when settings is IMoqPortSettingsModel:
                    device = _deviceFactory[DeviceType.Moq];
                    break;

                default:
                    device = null;
                    break;
            }
            device?.Setup(settings);
            return device;
        }

        public ICommunicationDevice CreateDevice(string name)
        {
            ICommunicationDevice device;

            var deviceSettings = GetSettingsByDeviceName(name ?? "");

            switch (deviceSettings)
            {
                case SerialPortSettingsModel _:
                    device = _deviceFactory[DeviceType.Serial];
                    break;

                case IMoqPortSettingsModel _:
                    device = _deviceFactory[DeviceType.Moq];
                    break;

                default:
                    string err = $@"ERROR: CommunicationDeviceFactory - no device with name ""{name}"" exists.";
                    Debug.WriteLine(err);
                    return null;
            }

            device?.Setup(deviceSettings);
            return device;
        }

        private ICommunicationDeviceSettings GetSettingsByDeviceName(string name) =>
            _settings?.ForTypeOf<ICommunicationDeviceSettingsContainer>()
                     ?.GetSettingsByDeviceName(name.ToUpperInvariant())
            ?? throw new System.ArgumentOutOfRangeException(nameof(name), $"There is no device with {name} name!");
    }
}