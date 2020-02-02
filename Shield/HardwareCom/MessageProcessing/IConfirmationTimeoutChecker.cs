using Shield.HardwareCom.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IConfirmationTimeoutChecker
    {
        long Timeout { get; set; }
        int NoTimeoutValue { get; }

        void AddConfirmation(IMessageHWComModel confirmation);

        void AddToCheckingQueue(IMessageHWComModel message);
        Task CheckUnconfirmedMessagesContinousAsync();
        bool IsExceeded(IMessageHWComModel message, IMessageHWComModel confirmation = null);

        BlockingCollection<IMessageHWComModel> ProcessedMessages();
        void ProcessMessageConfirmedBy(IMessageHWComModel confirmation);
    }
}