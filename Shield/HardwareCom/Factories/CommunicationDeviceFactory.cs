using Autofac.Features.Indexed;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using System.Diagnostics;

namespace Shield.HardwareCom.Factories
{
    /// <summary>
    /// Gives a chosen type of communication device based on DeviceType provided
    /// Takes optional parameters or a configuration directly from AppSettings (loaded from file)
    /// </summary>

    public class CommunicationDeviceFactory : ICommunicationDeviceFactory
    {
        private ISettings _settings;
        private IIndex<DeviceType, ICommunicationDevice> _deviceFactory;

        public CommunicationDeviceFactory(IIndex<DeviceType, ICommunicationDevice> deviceFactory,
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
                case var _ when settings is ISerialPortSettingsModel:
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
            if(string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentOutOfRangeException(nameof(name), $"There is no device with {name} name!");

            name = name.ToUpperInvariant();
            
            var deviceSettings = _settings.ForTypeOf<ICommunicationDeviceSettingsContainer>().GetSettingsByDeviceName(name)
                ?? throw new System.ArgumentOutOfRangeException(nameof(name), $"There is no device with {name} name!");

            ICommunicationDevice device;

            switch (deviceSettings)
            {
                case ISerialPortSettingsModel settings:
                device = _deviceFactory[DeviceType.Serial];                
                break;

                case IMoqPortSettingsModel settings:
                device = _deviceFactory[DeviceType.Moq];
                break;

                default:
                string err = $@"ERROR: CommunicationDeviceFactory Device - no device with name ""{name}"" exists.";
                Debug.WriteLine(err);
                return null;
            }
            
            return device.Setup(deviceSettings)
                ? device
                : null;
        }
    }
}