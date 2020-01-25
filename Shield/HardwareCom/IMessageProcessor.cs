using Shield.HardwareCom.Models;
using System.Collections.Concurrent;

namespace Shield.HardwareCom
{
    public interface IMessageProcessor
    {
        bool IsProcessingMessages { get; }

        void AddMessageToProcess(IMessageHWComModel message);

        BlockingCollection<IMessageHWComModel> GetProcessedMessages();

        void StartProcessingMessages();

        void StartProcessingMessagesContinous();

        void StopProcessingMessages();
        void SwitchSourceCollection(BlockingCollection<IMessageHWComModel> newSourceCollection);
        bool TryProcess(IMessageHWComModel messageToProcess, out IMessageHWComModel processedMessage);
    }
}