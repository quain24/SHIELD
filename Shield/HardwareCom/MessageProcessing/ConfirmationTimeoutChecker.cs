using Shield.Enums;
using Shield.Extensions;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    internal class ConfirmationTimeoutChecker : IConfirmationTimeoutChecker, IDisposable
    {
        private readonly SortedDictionary<long, IMessageModel> _storage = new SortedDictionary<long, IMessageModel>();
        private readonly BlockingCollection<IMessageModel> _processedMessages = new BlockingCollection<IMessageModel>();
        private readonly BlockingCollection<IMessageModel> _processedConfirmations = new BlockingCollection<IMessageModel>();
        private readonly Dictionary<string, IMessageModel> _confirmations = new Dictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ITimeoutCheck _timeoutCheck;
        private int _checkinterval = 0;

        private object _processLock = new object();
        private bool _isProcessing = false;
        private bool _disposed = false;

        private string _currentlyProcessingId = string.Empty;
        private readonly ReaderWriterLockSlim _currentlyProcessingIdLock = new ReaderWriterLockSlim();

        private CancellationTokenSource _processingCTS = new CancellationTokenSource();

        public int Timeout => _timeoutCheck.Timeout;

        public ConfirmationTimeoutChecker(ITimeoutCheck timeoutCheck)
        {
            _timeoutCheck = timeoutCheck ?? throw new ArgumentNullException(nameof(timeoutCheck),
                    "ConfirmationTimeoutChecker - ConfirmationTimeoutChecker: passed NULL instead of proper timeout checking object");
            _checkinterval = CalcCheckInterval(_timeoutCheck.Timeout);
        }

        public async Task CheckUnconfirmedMessagesContinousAsync()
        {
            if (!CanStartCheckingUnconfirmedMessages())
                return;

            Debug.WriteLine("ConfirmationTimeoutChecker: Started checking timeouts of sent messages.");

            try
            {
                while (!_processingCTS.IsCancellationRequested)
                {
                    if (!CheckNextUnconfirmedMessage()) // Is true when checked message was a timeout one - if so, immediately check next one.
                        await WaitForNextIteration().ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                if (!IsCheckUnconfirmedMessagesContinousAsyncCancelledProperly(e))
                    throw;
            }
        }

        private bool CanStartCheckingUnconfirmedMessages()
        {
            if (!_isProcessing)
                lock (_processLock)
                    return _isProcessing ? false : _isProcessing = true;
            return false;
        }

        private bool CheckNextUnconfirmedMessage()
        {
            IMessageModel message = GetNextUnconfirmedMessage();

            using (_currentlyProcessingIdLock.Read())
                if (message is null || _currentlyProcessingId == message.Id)
                    return false;

            ClearTimeoutError(message);

            if (IsTimeoutExceeded(message))
            {
                HandleExceededTimeout(message);
                return true;
            }
            return false;
        }

        private IMessageModel GetNextUnconfirmedMessage() =>
            _storage.FirstOrDefault().Value ?? null;

        private Task WaitForNextIteration() =>
            Task.Delay(_checkinterval, _processingCTS.Token);

        public bool IsTimeoutExceeded(IMessageModel message, IMessageModel confirmation = null)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message), "TimeoutChecker - Check: Cannot check timeout for NULL.");

            confirmation = confirmation is null ? GetConfirmationOf(message) : null;

            bool isTimeoutExceeded = confirmation is null
                ? _timeoutCheck.IsExceeded(message)
                : _timeoutCheck.IsExceeded(message, confirmation);

            return isTimeoutExceeded;
        }

        public IMessageModel GetConfirmationOf(IMessageModel message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message), "ConfirmationTimeoutchecker - isConfirmed: Cannot check NULL object");            
            _confirmations.TryGetValue(message.Id, out IMessageModel output);
            return output;
        }

        private void HandleExceededTimeout(IMessageModel message)
        {
            SetTimeoutError(message);
            if (GetConfirmationOf(message) is null)
                SetNoConfirmatioError(message);

            _processedMessages.Add(message);
            _storage.Remove(_storage.Keys.First());
            Console.WriteLine($@"ConfirmationTimeoutChecker: {message.Id} - Confirmation timeout exceeded.");
            _processingCTS.Token.ThrowIfCancellationRequested();
        }

        private bool IsCheckUnconfirmedMessagesContinousAsyncCancelledProperly(Exception e)
        {
            lock (_processLock)
                _isProcessing = false;
            return e is TaskCanceledException || e is OperationCanceledException;
        }

        public void StopCheckingUnconfirmedMessages()
        {
            _processingCTS.Cancel();
            _processingCTS = new CancellationTokenSource();
        }

        public void AddToCheckingQueue(IMessageModel message)
        {
            if (message is null)
                return;
            _storage.Add(message.Timestamp, message);
        }

        public void AddConfirmation(IMessageModel confirmation)
        {
            if (confirmation is null)
                return;

            _confirmations.Add(confirmation.Id, confirmation);
            CheckMessageConfirmedBy(confirmation);
        }

        private void CheckMessageConfirmedBy(IMessageModel confirmation)
        {
            if (confirmation is null) throw new ArgumentNullException(nameof(confirmation));

            IMessageModel message = _storage.FirstOrDefault((val) => val.Value.Id == confirmation.Id).Value;

            using (_currentlyProcessingIdLock.Write())
                _currentlyProcessingId = message is null ? string.Empty : message.Id;

            if (message is null)
            {
                Debug.WriteLine($@"ConfirmationTimeoutChecker: confirmation {confirmation.Id} tried to confirm nonexistent message.");
                confirmation.Errors |= Errors.ConfirmedNonexistent;
            }
            else
            {
                message = IsTimeoutExceeded(message, confirmation) ? SetTimeoutError(message) : message;
                message.IsConfirmed = true;
                _processedMessages.Add(message);
            }
            _processedConfirmations.Add(confirmation);

            using (_currentlyProcessingIdLock.Write())
                _currentlyProcessingId = string.Empty;
        }

        public BlockingCollection<IMessageModel> ProcessedMessages() => _processedMessages;

        public BlockingCollection<IMessageModel> ProcessedConfirmations() => _processedConfirmations;

        private IMessageModel SetTimeoutError(IMessageModel message)
        {
            message.Errors |= Errors.ConfirmationTimeout;
            return message;
        }

        private IMessageModel ClearTimeoutError(IMessageModel message)
        {
            message.Errors &= ~Errors.ConfirmationTimeout;
            return message;
        }

        private IMessageModel SetNoConfirmatioError(IMessageModel message)
        {
            message.Errors |= Errors.NotConfirmed;
            return message;
        }

        private IMessageModel ClearNoConfirmationError(IMessageModel message)
        {
            message.Errors &= ~Errors.NotConfirmed;
            return message;
        }

        /// <summary>
        /// Calculates timeout checking interval from given timeout value
        /// </summary>
        /// <param name="timeout">Calculating checking interval from this value</param>
        /// <returns>Timeout checking interval</returns>
        private int CalcCheckInterval(int timeout)
        {
            switch (timeout)
            {
                case var _ when timeout <= 100:
                    return 100;

                case var _ when timeout <= 3000:
                    return 500;

                case var _ when timeout <= 5000:
                    return 1000;

                default:
                    return 2000;
            }
        }

        #region IDispose implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _processedConfirmations?.Dispose();
                    _processedMessages?.Dispose();
                    StopCheckingUnconfirmedMessages();
                    _processingCTS?.Dispose();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }

        #endregion IDispose implementation
    }
}