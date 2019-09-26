using Shield.CommonInterfaces;
using Shield.Enums;
using System.IO.Ports;

namespace Shield.HardwareCom.Factories
{
    public interface ICommunicationDeviceFactory
    {
        ICommunicationDevice Device(DeviceType type);
    }
}