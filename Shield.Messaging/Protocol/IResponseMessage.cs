using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public interface IResponseMessage
    {
        string Target { get; }
        Timestamp Timestamp { get; }
    }
}