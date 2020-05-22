using Shield.Messaging.RawData;

namespace Shield.Messaging.Commands
{
    public interface ICommand
    {
        IPart ID { get; }
        IPart HostID { get; }
        IPart Type { get; }
        IPart Data { get; }
        Timestamp Timestamp { get; }
    }
}