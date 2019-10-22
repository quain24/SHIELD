using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public interface IMessageModel : IEnumerable<ICommandModel>
    {
        long Timestamp { get; set; }
        bool IsBeingSent { get; set; }
        bool IsBeingReceived { get; set; }
        bool IsIncoming { get; set; }
        bool IsOutgoing { get; set; }
        bool IsTransmissionCompleted { get; set; }
        int CommandCount { get; }
        string Id { get; set; }

        string AssaignID(string id = "");

        bool Add(ICommandModel command);

        bool Remove(ICommandModel command);
    }
}