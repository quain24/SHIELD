using Shield.Enums;
using System.Collections.Generic;
using static Shield.HardwareCom.Models.MessageModel;

namespace Shield.HardwareCom.Models
{
    public interface IMessageModel : IEnumerable<ICommandModel>
    {
        long Timestamp { get; set; }
        bool IsTransmissionCompleted { get; set; }
        int CommandCount { get; }
        string Id { get; set; }
        List<ICommandModel> Commands { get; }

        string AssaignID(string id = "");

        bool Add(ICommandModel command);

        bool Remove(ICommandModel command);
    }
}