using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public interface IConfirmable
    {
        string ID { get; }
        Timestamp Timestamp { get; }
    }
}