using Shield.Messaging.Commands.Parts;

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