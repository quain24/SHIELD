using Shield.HardwareCom.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IConfirmationTimeoutChecker
    {
        long Timeout { get; set; }
        int NoTimeoutValue { get; }

        void AddConfirmation(IMessageModel confirmation);

        void AddToCheckingQueue(IMessageModel message);
        Task CheckUnconfirmedMessagesContinousAsync();
        bool IsExceeded(IMessageModel message, IMessageModel confirmation = null);

        BlockingCollection<IMessageModel> ProcessedMessages();
        void ProcessMessageConfirmedBy(IMessageModel confirmation);
    }
}