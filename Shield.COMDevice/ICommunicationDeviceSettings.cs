using Shield.CommonInterfaces;

namespace Shield.COMDevice
{
    public interface ICommunicationDeviceSettings : ISetting
    {
        string Name { get; }
        int CompletitionTimeout { get; set; }
        int ConfirmationTimeout { get; set; }
    }
}