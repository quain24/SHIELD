using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface IMessengerFactory
    {
        HardwareCom.IMessenger CreateMessangerUsing(ICommunicationDevice device);
    }
}