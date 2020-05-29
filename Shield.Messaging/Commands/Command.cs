using Shield.Messaging.Commands.Parts;

namespace Shield.Messaging.Commands
{
    public class Command : ICommand
    {
        public Command(IPart id, IPart hostID, IPart type, IPart data)
        {
            ID = id;
            HostID = hostID;
            Type = type;
            Data = data;
            Timestamp = TimestampFactory.Timestamp;
        }

        public IPart ID { get; }
        public IPart HostID { get; }
        public IPart Type { get; }
        public IPart Data { get; }
        public Timestamp Timestamp { get; }
    }
}