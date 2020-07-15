using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.States;
using Shield.Timestamps;

namespace Shield.Messaging.Commands
{
    public interface ICommand
    {
        IPart ID { get; }
        IPart HostID { get; }
        IPart Target { get; }
        IPart Order { get; }
        IPart Data { get; }
        Timestamp Timestamp { get; }
        ErrorState ErrorState { get; }
        bool IsValid { get; }
    }
}