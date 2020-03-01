using Shield.Extensions;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    internal class CommandIngester : ICommandIngester, IDisposable
    {
        private readonly IMessageFactory _msgFactory;
        private readonly ICompleteness _completness;
        private readonly ITimeoutCheck _completitionTimeoutChecker;

        private readonly BlockingCollection<ICommandModel> _awaitingQueue = new BlockingCollection<ICommandModel>();
        private readonly ConcurrentDictionary<string, IMessageModel> _incompleteMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _completedMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly BlockingCollection<IMessageModel> _processedMessages = new BlockingCollection<IMessageModel>();
        private readonly BlockingCollection<ICommandModel> _errCommands = new BlockingCollection<ICommandModel>();

        private readonly ReaderWriterLockSlim _messageProcessingLock = new ReaderWriterLockSlim();
        private readonly object _processingLock = new object();
        private readonly object _timeoutCheckLock = new object();
        private bool _isProcessing = false;
        private bool _isTimeoutChecking = false;
        private bool _disposed = false;

        private CancellationTokenSource _cancelProcessingCTS = new CancellationTokenSource();
        private CancellationTokenSource _cancelTimeoutCheckCTS = new CancellationTokenSource();

        /// <summary>
        /// Returns true if commands are being processed or object waits for new commands to be processed
        /// </summary>
        public bool IsProcessingCommands => _isProcessing;

        /// <summary>
        /// Will create messages from received commands
        /// </summary>
        /// <param name="messageFactory">Message factory delegate</param>
        /// <param name="completeness">State check - checks if message is completed</param>
        /// <param name="completitionTimeout">State check - optional - checks if completition time is exceeded</param>
        public CommandIngester(IMessageFactory messageFactory, ICompleteness completeness, ITimeoutCheck completitionTimeout)
        {
            _msgFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            _completness = completeness ?? throw new ArgumentNullException(nameof(completeness));
            _completitionTimeoutChecker = completitionTimeout ?? throw new ArgumentNullException(nameof(completitionTimeout));
        }

        /// <summary>
        /// Add single command to be processed / injected into corresponding / new message in internal collection.
        /// Thread safe
        /// </summary>
        /// <param name="command">command object to be ingested</param>
        /// <returns>true if success</returns>
        public bool AddCommandToProcess(ICommandModel command)
        {
            if (command is null)
                return false;
            _awaitingQueue.Add(command);
            Console.WriteLine($@"CommandIngester - Command {command.Id} added to be processed.");
            return true;
        }

        /// <summary>
        /// Start processing of commands that were added to internal collection by <code>AddCommandToProcess</code>
        /// Thread safe - only single instance can be started at once.
        /// </summary>
        public void StartProcessingCommands()
        {
            try
            {
                if (!CanStartProcessing())
                    return;

                ProcessCommandsContinously();
            }
            catch (Exception e)
            {
                if (!IsStartProcessingCommandProperlyCancelled(e))
                    throw;
            }
        }

        private bool CanStartProcessing()
        {
            lock (_processingLock)
                return _isProcessing
                    ? false
                    : _isProcessing = true;
        }

        private void ProcessCommandsContinously()
        {
            Debug.WriteLine("CommandIngester - Command processing started");
            while (true)
            {
                ICommandModel command = GetNextCommand();
                SetIdAsUsedUp(command.Id);
                ProcessCommand(command);
                _cancelProcessingCTS.Token.ThrowIfCancellationRequested();
            }
        }

        private ICommandModel GetNextCommand() =>
            _awaitingQueue.Take(_cancelProcessingCTS.Token)
            ?? throw new NullReferenceException("GetNextCommand returned NULL - it really shouldn't");

        private void SetIdAsUsedUp(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
                Helpers.IdGenerator.UsedThisID(id);
        }

        public void ProcessCommand(ICommandModel command)
        {
            using (_messageProcessingLock.Write())
            {
                if (IsMessageAlreadyComplete(command.Id))
                {
                    HandleBadCommand(command);
                    return;
                }

                IMessageModel message;

                message = GetMessageToWorkWith(command);

                message.Add(command);

                if (_completness.IsComplete(message))
                    PushFromIncompleteToProcessed(message);
                else if (_completitionTimeoutChecker.IsExceeded(message))
                    HandleMessageTimeout(message);
            }
        }

        private bool IsMessageAlreadyComplete(string id) => _completedMessages.ContainsKey(id);

        private void HandleBadCommand(ICommandModel command)
        {
            _errCommands.Add(command);
            Debug.WriteLine("CommandIngester - ERROR - tried to add new command to completed / null message");
        }

        private IMessageModel GetMessageToWorkWith(ICommandModel command) =>
            IsFoundInIncomplete(command.Id)
                ? _incompleteMessages[command.Id]
                : CreateNewIncomingMessage(command.Id);

        private bool IsFoundInIncomplete(string messageId) =>
            _incompleteMessages.ContainsKey(messageId);

        private IMessageModel CreateNewIncomingMessage(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be empty", nameof(id));

            IMessageModel message = _msgFactory.CreateNew(direction: Enums.Direction.Incoming, id: id);
            Debug.WriteLine($@"CommandIngester - new {id} message created");
            _incompleteMessages.TryAdd(message.Id, message);
            return message;
        }

        private void PushFromIncompleteToProcessed(IMessageModel message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            _completedMessages.TryAdd(message.Id, message);
            _incompleteMessages.TryRemove(message.Id, out _);
            _processedMessages.Add(message);
            Debug.WriteLine($@"CommandIngester - Message {message.Id} was processed, adding to processed messages collection");
        }

        private void HandleMessageTimeout(IMessageModel message)
        {
            if (message is null) return;
            message.Errors |= Enums.Errors.CompletitionTimeout;
            PushFromIncompleteToProcessed(message);
            Debug.WriteLine($@"ContinousTimeoutChecker - Error - message {message.Id} completition timeout");
        }

        private bool IsStartProcessingCommandProperlyCancelled(Exception e)
        {
            lock (_processingLock)
                _isProcessing = false;
            Debug.WriteLine("CommandIngester - StartProcessingCommands ENDED");
            return e is TaskCanceledException || e is OperationCanceledException;
        }

        /// <summary>
        /// Stops processing commands
        /// </summary>
        public void StopProcessingCommands()
        {
            _cancelProcessingCTS.Cancel();
            _cancelProcessingCTS = new CancellationTokenSource();
        }

        public async Task StartTimeoutCheckAsync(int interval = 0)
        {
            try
            {
                if (!CanStartTiemoutCheck())
                    return;

                int checkInterval = CalculateCheckInterval(interval);

                while (true)
                {
                    _cancelTimeoutCheckCTS.Token.ThrowIfCancellationRequested();
                    ProcessTimeouts();
                    await Task.Delay(checkInterval, _cancelTimeoutCheckCTS.Token).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                if (!IsTimeoutCheckCorrectlyCancelled(e))
                    throw;
            }
        }

        private bool CanStartTiemoutCheck()
        {
            if (_completitionTimeoutChecker.Timeout == 0)
                return false;

            lock (_timeoutCheckLock)
            {
                if (!_isTimeoutChecking)
                    return _isTimeoutChecking = true;
                else
                    return false;
            }
        }

        // TODO rethink this method
        private int CalculateCheckInterval(long timeout)
        {
            switch (timeout)
            {
                case var _ when timeout <= _completitionTimeoutChecker.Timeout:
                return 100;

                case var _ when timeout <= 1000:
                return 250;

                case var _ when timeout <= 3000:
                return 500;

                case var _ when timeout > 5000:
                return 1000;

                default:
                return _completitionTimeoutChecker.NoTimeoutValue;
            }
        }

        private void ProcessTimeouts()
        {
            if (_incompleteMessages.Count > 250)
                HandleTimeoutsParallel(GetlistOfUnconfirmedMessagesParallel());
            else
                HandleTimeouts(GetlistOfUnconfirmedMessages());
        }

        private void HandleTimeoutsParallel(List<IMessageModel> listOfTimeouts)
        {
            using (_messageProcessingLock.Write())
                listOfTimeouts.AsParallel().ForAll(m => HandleMessageTimeout(m));
        }

        private void HandleTimeouts(List<IMessageModel> listOfTimeouts)
        {
            using (_messageProcessingLock.Write())
                listOfTimeouts.ForEach(m => HandleMessageTimeout(m));
        }

        private List<IMessageModel> GetlistOfUnconfirmedMessages()
        {
            return _incompleteMessages
                 .Select(kvp => kvp.Value)
                 .Where(m => _completitionTimeoutChecker.IsExceeded(m))
                 .ToList();
        }

        private List<IMessageModel> GetlistOfUnconfirmedMessagesParallel()
        {
            return _incompleteMessages
                 .AsParallel()
                 .Select(kvp => kvp.Value)
                 .Where(m => _completitionTimeoutChecker.IsExceeded(m))
                 .ToList();
        }

        private bool IsTimeoutCheckCorrectlyCancelled(Exception e)
        {
            lock (_timeoutCheckLock)
                _isTimeoutChecking = false;

            return (e is TaskCanceledException || e is OperationCanceledException) ? true : false;
        }

        public void StopTimeoutCheck()
        {
            _cancelTimeoutCheckCTS.Cancel();
            _cancelTimeoutCheckCTS = new CancellationTokenSource();
        }

        /// <summary>
        /// Gives a thread safe collection of completed and timeout messages for further processing
        /// </summary>
        /// <returns></returns>
        public BlockingCollection<IMessageModel> GetProcessedMessages()
        {
            Console.WriteLine($@"CommandIngester - Requested Processed Messages ({_processedMessages.Count} available)");
            return _processedMessages;
        }

        public Dictionary<string, IMessageModel> GetIncompletedMessages() => new Dictionary<string, IMessageModel>(_incompleteMessages);

        /// <summary>
        /// Gives a thread safe collection of commands that were received, but corresponding messages were already completed or completition timeout
        /// was exceeded.
        /// </summary>
        /// <returns></returns>
        public BlockingCollection<ICommandModel> GetErrAlreadyCompleteOrTimeout() => _errCommands;

        #region IDisposable implementation

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
                    _awaitingQueue?.Dispose();
                    _processedMessages?.Dispose();
                    _errCommands?.Dispose();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }

        #endregion IDisposable implementation
    }
}