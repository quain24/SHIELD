using Autofac.Features.Indexed;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using System.Diagnostics;

namespace Shield.HardwareCom.Factories
{
    /// <summary>
    /// Gives a choosen type of communication device based on DeviceType provided
    /// Takes optional parameters or a configuration directly from AppSettings (loaded from file)
    /// </summary>

    public class CommunicationDeviceFactory : ICommunicationDeviceFactory
    {
        private IAppSettings _appSettings;
        private IIndex<DeviceType, ICommunicationDevice> _deviceFactory;

        public CommunicationDeviceFactory(IIndex<DeviceType, ICommunicationDevice> deviceFactory,
                                          IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _deviceFactory = deviceFactory;
        }

        public ICommunicationDevice Device(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.Serial:
                    ISerialPortSettingsModel settings = (ISerialPortSettingsModel)_appSettings.GetSettingsFor(SettingsType.SerialDevice);
                    ICommunicationDevice device = _deviceFactory[type];
                    if (device.Setup(settings))
                        return device;
                    else
                        break;

                case DeviceType.Moq:
                    IMoqPortSettingsModel settings2 = (IMoqPortSettingsModel)_appSettings.GetSettingsFor(SettingsType.MoqDevice);
                    ICommunicationDevice device2 = _deviceFactory[type];
                    if (device2.Setup(settings2))
                        return device2;
                    else
                        break;

                default:
                    string err = $@"ERROR: CommunicationDeviceFactory Device - no device at ""{type}"" position in deviceType enum";
                    Debug.WriteLine(err);
                    return null;
            }
            return null;
        }
    }
}