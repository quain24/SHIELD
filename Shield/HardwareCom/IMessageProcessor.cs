using Shield.HardwareCom.Models;
using System.Collections.Concurrent;

namespace Shield.HardwareCom
{
    public interface IMessageProcessor
    {
        bool IsProcessingMessages { get; }

        void AddMessageToProcess(IMessageModel message);

        BlockingCollection<IMessageModel> GetProcessedMessages();

        void StartProcessingMessages();

        void StartProcessingMessagesContinous();

        void StopProcessingMessages();
        void SwitchSourceCollection(BlockingCollection<IMessageModel> newSourceCollection);
        bool TryProcess(IMessageModel messageToProcess, out IMessageModel processedMessage);
    }
}