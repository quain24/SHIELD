using Shield.HardwareCom.Models;
using System.Collections.Concurrent;

namespace Shield.HardwareCom.CommandProcessing
{
    public interface ICommandIngester
    {
        bool IsProcessingCommands { get; }

        void AddCommandToProcess(ICommandModel command);

        BlockingCollection<ICommandModel> GetErrAlreadyCompleteOrTimeout();

        ConcurrentDictionary<string, IMessageModel> GetIncompletedMessages();

        BlockingCollection<IMessageModel> GetReceivedMessages();
        void Process(ICommandModel command);
        void PushFromIncompleteToProcessed(IMessageModel message);

        void StartProcessingCommands();

        void StopProcessingCommands();

        void SwitchSourceCollectionTo(BlockingCollection<ICommandModel> newSourceCollection);
        void StartProcessingCommandsTillBufferEnds();
    }
}