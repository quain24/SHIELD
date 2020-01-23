using Shield.HardwareCom.Models;
using System.Collections.Concurrent;

namespace Shield.HardwareCom
{
    public interface ICommandIngester
    {
        bool AddCommandToProcess(ICommandModel command);
        BlockingCollection<ICommandModel> GetErrAlreadyCompleteOrTimeout();
        BlockingCollection<IMessageHWComModel> GetProcessedMessages();
        void StartProcessingCommands();
        void StopProcessingCommands();
        bool TryIngest(ICommandModel incomingCommand, out IMessageHWComModel message);
    }
}