using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public interface IMessageModel : IEnumerable<ICommandModel>
    {
        long Timestamp { get; set; }
        int CommandCount { get; }
        string Id { get; set; }
        List<ICommandModel> Commands { get; }

        string AssaignID(string id = "");

        bool Add(ICommandModel command);

        bool Remove(ICommandModel command);

        bool Replace(ICommandModel target, ICommandModel replacement);
    }
}