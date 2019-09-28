using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public interface IMessageModel : IEnumerable<ICommandModel>
    {
        bool IsBeingSent { get; set; }
        bool IsBeingReceived { get; set; }
        bool IsIncoming { get; set; }
        bool IsOutgoing { get; set; }
        int CommandCount { get; }

        void Add(ICommandModel command);

        bool Remove(int id);

        bool Remove(ICommandModel command);
    }
}