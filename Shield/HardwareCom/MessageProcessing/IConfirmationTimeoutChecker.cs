using Shield.HardwareCom.Models;
using System.Collections.Concurrent;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IConfirmationTimeoutChecker
    {
        long Timeout { get; set; }

        void AddConfirmation(IMessageHWComModel confirmation);

        void AddToCheckingQueue(IMessageHWComModel message);

        bool IsExceeded(IMessageHWComModel message, IMessageHWComModel confirmation = null);

        BlockingCollection<IMessageHWComModel> ProcessedMessages();
    }
}