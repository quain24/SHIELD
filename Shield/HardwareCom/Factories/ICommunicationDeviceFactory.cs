using Shield.CommonInterfaces;
using Shield.Enums;

namespace Shield.HardwareCom.Factories
{
    public interface ICommunicationDeviceFactory
    {
        ICommunicationDevice CreateDevice(ICommunicationDeviceSettings settings);
        ICommunicationDevice Device(DeviceType type, int portnumber = 0);
    }
}