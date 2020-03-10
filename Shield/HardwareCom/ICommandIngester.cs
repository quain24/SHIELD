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
        BlockingCollection<IMessageModel> GetReceivedMessages();
        void StartProcessingCommands();
        Task StartTimeoutCheckAsync();
        void StopProcessingCommands();
        void StopTimeoutCheck();
    }
}