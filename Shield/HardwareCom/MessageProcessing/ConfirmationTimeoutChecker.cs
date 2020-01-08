using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shield.Enums;

namespace Shield.HardwareCom.MessageProcessing
{
    public class ConfirmationTimeoutChecker : IConfirmationTimeoutChecker
    {
        private BlockingCollection<IMessageHWComModel> _storage = new BlockingCollection<IMessageHWComModel>();
        private BlockingCollection<IMessageHWComModel> _processedMessages = new BlockingCollection<IMessageHWComModel>();
        private Dictionary<string, IMessageHWComModel> _confirmations = new Dictionary<string, IMessageHWComModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ITimeoutCheck _timeoutCheck;
        private int _checkinterval = 0;
        private readonly int _noTimeout;

        public long Timeout
        {
            get => _timeoutCheck.Timeout;
            set
            {
                if (_timeoutCheck is null)
                    return;
                _timeoutCheck.Timeout = value;
                CalcCheckInterval(_timeoutCheck.Timeout);
            }
        }

        public ConfirmationTimeoutChecker(ITimeoutCheck timeoutCheck)
        {
            if(timeoutCheck is null)
                throw new ArgumentNullException(nameof(timeoutCheck),
                    "ConfirmationTimeoutChecker - ConfirmationTimeoutChecker: passed NULL instead of proper timeout checking object");

            _timeoutCheck = timeoutCheck;

            _noTimeout = _timeoutCheck.NoTimeoutValue;
            _checkinterval = CalcCheckInterval(Timeout);
        }

        public void AddToCheckingQueue(IMessageHWComModel message)
        {
            if (message is null)
                return;
            _storage.Add(message);
        }

        public void AddConfirmation(IMessageHWComModel confirmation)
        {
            if (confirmation is null)
                return;

            _confirmations.Add(confirmation.Id, confirmation);
        }

        public BlockingCollection<IMessageHWComModel> ProcessedMessages()
        {
            return _processedMessages;
        }

        public bool IsExceeded(IMessageHWComModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "TimeoutChecker - Check: Cannot check timeout for NULL.");

            if (Timeout == _noTimeout)
                return false;

            ClearTimeoutError(message);

            bool isTimeoutExceeded = _confirmations.ContainsKey(message.Id) ?
                _timeoutCheck.IsExceeded(message, _confirmations[message.Id]):
                _timeoutCheck.IsExceeded(message);

            message = isTimeoutExceeded ? SetTimeoutError(message) : message ;

            return isTimeoutExceeded;
        }

        private IMessageHWComModel SetTimeoutError(IMessageHWComModel message)
        {
            if(message.Errors.HasFlag(Errors.ConfirmationTimeout))
                message.Errors &= ~Errors.ConfirmationTimeout;
            message.Errors |= Errors.ConfirmationTimeout;
            return message;
        }

        private IMessageHWComModel ClearTimeoutError(IMessageHWComModel message)
        {
            if(message.Errors.HasFlag(Errors.ConfirmationTimeout))
                message.Errors &= ~Errors.ConfirmationTimeout;
            return message;
        }

        private int CalcCheckInterval(long timeout)
        {
            switch (timeout)
            {
                case var _ when timeout <= 0:
                return _noTimeout;

                case var _ when timeout <= 100:
                return 10;

                case var _ when timeout <= 1000:
                return 100;

                case var _ when timeout > 1000:
                return 250;

                default:
                return _noTimeout;
            }
        }
    }
}