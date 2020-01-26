using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public class ConfirmationTimeoutChecker : IConfirmationTimeoutChecker
    {
        private SortedDictionary<long, IMessageHWComModel> _storage = new SortedDictionary<long, IMessageHWComModel>();
        private BlockingCollection<IMessageHWComModel> _processedMessages = new BlockingCollection<IMessageHWComModel>();
        private BlockingCollection<IMessageHWComModel> _processedConfirmations = new BlockingCollection<IMessageHWComModel>();
        private Dictionary<string, IMessageHWComModel> _confirmations = new Dictionary<string, IMessageHWComModel>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ITimeoutCheck _timeoutCheck;
        private int _checkinterval = 0;
        private readonly int _noTimeout;

        private object _processLock = new object();
        private bool _isProcessing = false;

        private string _currentlyProcessingId = string.Empty;
        private ReaderWriterLockSlim _currentlyProcessingIdLock = new ReaderWriterLockSlim();

        private CancellationTokenSource _processingCTS = new CancellationTokenSource();

        public long Timeout
        {
            get => _timeoutCheck.Timeout;
            set
            {
                _timeoutCheck.Timeout = value;
                CalcCheckInterval(_timeoutCheck.Timeout);
            }
        }

        public ConfirmationTimeoutChecker(ITimeoutCheck timeoutCheck)
        {
            if (timeoutCheck is null)
                throw new ArgumentNullException(nameof(timeoutCheck),
                    "ConfirmationTimeoutChecker - ConfirmationTimeoutChecker: passed NULL instead of proper timeout checking object");

            _timeoutCheck = timeoutCheck;

            _noTimeout = _timeoutCheck.NoTimeoutValue;
            _checkinterval = CalcCheckInterval(Timeout);
        }

        public IMessageHWComModel GetConfirmationOf(IMessageHWComModel message)
        {
            if(message is null) throw new ArgumentNullException(nameof(message), "ConfirmationTimeoutchecker - isConfirmed: Cannot check NULL object");

            IMessageHWComModel output;
            _confirmations.TryGetValue(message.Id, out output);
            return output;
        }

        public bool IsExceeded(IMessageHWComModel message, IMessageHWComModel confirmation = null)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "TimeoutChecker - Check: Cannot check timeout for NULL.");

            if (Timeout == _noTimeout)
                return false;

            //ClearTimeoutError(message);
            confirmation = confirmation is null ? GetConfirmationOf(message) : null;

            bool isTimeoutExceeded = confirmation is null ?
                _timeoutCheck.IsExceeded(message) :
                _timeoutCheck.IsExceeded(message, confirmation);

            //message = isTimeoutExceeded ? SetTimeoutError(message) : message;

            return isTimeoutExceeded;
        }

        public async Task CheckUnconfirmedMessagesAsync()
        {
            if (!_isProcessing)
            {
                lock (_processLock)
                {
                    if (_isProcessing)
                        return;
                    _isProcessing = true;
                }
            }

            try
            {
                IMessageHWComModel message;

                while (_storage.Count > 0 && !_processingCTS.IsCancellationRequested)
                {
                    message = _storage.FirstOrDefault().Value;
                    if (message is null)
                        break;

                    ClearTimeoutError(message);

                    _currentlyProcessingIdLock.EnterReadLock();
                    if (_currentlyProcessingId == message.Id)
                    {
                        _currentlyProcessingIdLock.ExitReadLock();
                        _processingCTS.Token.ThrowIfCancellationRequested();
                        continue;
                    }
                    _currentlyProcessingIdLock.ExitReadLock();

                    if (IsExceeded(message))
                    {
                        SetTimeoutError(message);
                        _processedMessages.Add(message);
                        _storage.Remove(_storage.Keys.First());
                        _processingCTS.Token.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        await Task.Delay(_checkinterval, _processingCTS.Token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                lock (_processLock)
                {
                    _isProcessing = false;
                }
            }
        }

        public void StopCheckingUnconfirmedMessages()
        {
            _processingCTS.Cancel();
            _processingCTS = new CancellationTokenSource();
        }

        public void ProcessMessageConfirmedBy(IMessageHWComModel confirmation)
        {
            if(confirmation is null) throw new ArgumentNullException(nameof(confirmation));

            IMessageHWComModel message = _storage.FirstOrDefault((val) => val.Value.Id == confirmation.Id).Value;

            _currentlyProcessingIdLock.EnterWriteLock();
            _currentlyProcessingId = message is null ? string.Empty : message.Id;
            _currentlyProcessingIdLock.ExitWriteLock();

            if (message is null)
            {
                confirmation.Errors |= Errors.ConfirmedNonexistent;
            }
            else
            {
                message = IsExceeded(message, confirmation) ? SetTimeoutError(message) : message;
                message.IsConfirmed = true;
                _processedMessages.Add(message);
            }
            _processedConfirmations.Add(confirmation);

            _currentlyProcessingIdLock.EnterWriteLock();
            _currentlyProcessingId = string.Empty;
            _currentlyProcessingIdLock.ExitWriteLock();
        }

        public void AddToCheckingQueue(IMessageHWComModel message)
        {
            if (message is null)
                return;
            //_storage.Add(message.Id, message);
            _storage.Add(message.Timestamp, message);
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

        public BlockingCollection<IMessageHWComModel> ProcessedConfirmations()
        {
            return _processedConfirmations;
        }

        private IMessageHWComModel SetTimeoutError(IMessageHWComModel message)
        {
            if (message.Errors.HasFlag(Errors.ConfirmationTimeout))
                message.Errors &= ~Errors.ConfirmationTimeout;
            message.Errors |= Errors.ConfirmationTimeout;
            return message;
        }

        private IMessageHWComModel ClearTimeoutError(IMessageHWComModel message)
        {
            if (message.Errors.HasFlag(Errors.ConfirmationTimeout))
                message.Errors &= ~Errors.ConfirmationTimeout;
            return message;
        }

        private IMessageHWComModel SetNoConfirmatioError(IMessageHWComModel message)
        {
            if (message.Errors.HasFlag(Errors.NotConfirmed))
                message.Errors &= ~Errors.NotConfirmed;
            message.Errors |= Errors.NotConfirmed;
            return message;
        }

        private IMessageHWComModel ClearNoConfirmationError(IMessageHWComModel message)
        {
            if (message.Errors.HasFlag(Errors.NotConfirmed))
                message.Errors &= ~Errors.NotConfirmed;
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