using Shield.HardwareCom.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface ICommandIngester
    {
        void AddCommandToProcess(ICommandModel command);
        BlockingCollection<ICommandModel> GetErrAlreadyCompleteOrTimeout();
        ConcurrentDictionary<string, IMessageModel> GetIncompletedMessages();
        BlockingCollection<IMessageModel> GetReceivedMessages();
        void PushFromIncompleteToProcessed(IMessageModel message);
        void StartProcessingCommands();
        void StopProcessingCommands();
        void SwitchSourceCollectionTo(BlockingCollection<ICommandModel> newSourceCollection);
    }
}
