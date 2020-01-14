using Shield.HardwareCom.Models;
using System.Collections.Concurrent;

namespace Shield.HardwareCom
{
    public interface ICommandIngester
    {
        BlockingCollection<IMessageHWComModel> GetProcessedMessages();

        bool TryIngest(ICommandModel incomingCommand, out IMessageHWComModel message);
    }
}