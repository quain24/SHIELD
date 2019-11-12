using Shield.Enums;
using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public interface IMessageHWComModel : IEnumerable<ICommandModel>
    {
        List<ICommandModel> Commands { get; }
        bool Completed { get; set; }
        bool Confirmed { get; set; }
        bool Correct { get; set; }
        Direction Direction { get; set; }
        Errors Errors { get; set; }
        string Id { get; set; }
        long Timestamp { get; set; }
        bool Transfered { get; set; }
        MessageType Type { get; set; }

        bool Add(ICommandModel command);

        string AssaignID(string id);

        IEnumerator<ICommandModel> GetEnumerator();

        bool Remove(ICommandModel command);

        bool Replace(ICommandModel target, ICommandModel replacement);
    }
}