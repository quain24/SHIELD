using Shield.HardwareCom.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface ICommandIngester
    {
        bool AddCommandToProcess(ICommandModel command);
        BlockingCollection<ICommandModel> GetErrAlreadyCompleteOrTimeout();
        Dictionary<string, IMessageModel> GetIncompletedMessages();
        BlockingCollection<IMessageModel> GetProcessedMessages();
        void StartProcessingCommands();
        Task StartTimeoutCheck(int interval = 0);
        void StopProcessingCommands();
        void StopTimeoutCheck();
        bool TryIngest(ICommandModel incomingCommand, out IMessageModel message);
    }
}