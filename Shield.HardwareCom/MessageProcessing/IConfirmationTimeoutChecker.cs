using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IConfirmationTimeoutChecker
    {
        event EventHandler<IMessageModel> TimeoutOccured;

        int Timeout { get; }
        bool IsWorking { get; }

        void AddConfirmation(IMessageModel confirmation);

        void AddToCheckingQueue(IMessageModel message);

        Task CheckUnconfirmedMessagesContinousAsync();

        bool IsTimeoutExceeded(IMessageModel message, IMessageModel confirmation = null);

        BlockingCollection<IMessageModel> ProcessedMessages();

        void StopCheckingUnconfirmedMessages();
    }
}