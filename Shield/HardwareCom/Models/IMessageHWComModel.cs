using Shield.Enums;
using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public interface IMessageHWComModel : IEnumerable<ICommandModel>
    {
        List<ICommandModel> Commands { get; }
        bool IsCompleted { get; set; }
        bool IsConfirmed { get; set; }
        bool IsCorrect { get; }
        Direction Direction { get; set; }
        Errors Errors { get; set; }
        string Id { get; set; }
        long Timestamp { get; set; }
        bool IsTransfered { get; set; }
        MessageType Type { get; set; }
        int CommandCount { get; }

        bool Add(ICommandModel command);

        string AssaignID(string id);

        IEnumerator<ICommandModel> GetEnumerator();

        bool Remove(ICommandModel command);

        bool Replace(ICommandModel target, ICommandModel replacement);
    }
}