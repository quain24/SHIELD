using System.Collections.Concurrent;
using System.Collections.Generic;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface ICommandIngester
    {
        BlockingCollection<IMessageHWComModel> GetProcessedMessages();
        bool TryIngest(ICommandModel incomingCommand, out IMessageHWComModel message);
    }
}