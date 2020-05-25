using Shield.Messaging.Commands.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Commands
{
    public class Command : ICommand
    {
        public Command(IPart id, IPart hostID, IPart type, IPart data, Timestamp timestamp)
        {
            ID = id;
            HostID = hostID;
            Type = type;
            Data = data;
            Timestamp = timestamp;
        }

        public IPart ID { get; }
        public IPart HostID { get; }
        public IPart Type { get; }
        public IPart Data { get; }
        public Timestamp Timestamp { get; }
    }
}
