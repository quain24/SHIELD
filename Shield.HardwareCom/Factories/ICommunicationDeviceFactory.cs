using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface ICommunicationDeviceFactory
    {
        ICommunicationDevice CreateDevice(ICommunicationDeviceSettings settings);

        ICommunicationDevice CreateDevice(string name);
    }
}