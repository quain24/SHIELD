using Shield.HardwareCom.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IConfirmationTimeoutChecker
    {
        int Timeout { get;}

        void AddConfirmation(IMessageModel confirmation);

        void AddToCheckingQueue(IMessageModel message);

        Task CheckUnconfirmedMessagesContinousAsync();

        bool IsTimeoutExceeded(IMessageModel message, IMessageModel confirmation = null);

        BlockingCollection<IMessageModel> ProcessedMessages();
        void StopCheckingUnconfirmedMessages();
    }
}