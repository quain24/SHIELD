using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.CommandProcessing
{
    internal class CommandIngester : ICommandIngester, IDisposable
    {
        private readonly IMessageFactory _msgFactory;
        private readonly ICompleteness _completness;
        private readonly IIdGenerator _idGenerator;

        private BlockingCollection<ICommandModel> _awaitingQueue = new BlockingCollection<ICommandModel>();
        private readonly ConcurrentDictionary<string, IMessageModel> _incompleteMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _completedMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly BlockingCollection<IMessageModel> _processedMessages = new BlockingCollection<IMessageModel>();
        private readonly BlockingCollection<ICommandModel> _errCommands = new BlockingCollection<ICommandModel>();

        private readonly ReaderWriterLockSlim _messageProcessingLock = new ReaderWriterLockSlim();
        private readonly object _processingLock = new object();
        private bool _isProcessing = false;
        private bool _disposed = false;

        private CancellationTokenSource _cancelProcessingCTS = new CancellationTokenSource();

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
        public CommandIngester(IMessageFactory messageFactory, ICompleteness completeness, IIdGenerator idGenerator)
        {
            _msgFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            _completness = completeness ?? throw new ArgumentNullException(nameof(completeness));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        /// <summary>
        /// Add single command to be processed / injected into corresponding / new message in internal collection.
        /// Thread safe
        /// </summary>
        /// <param name="command">command object to be ingested</param>
        /// <returns>true if success</returns>
        public void AddCommandToProcess(ICommandModel command)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));
            _awaitingQueue.Add(command);
        }

        /// <summary>
        /// Start processing of commands that were added to internal collection by <code>AddCommandToProcess</code>
        /// Thread safe - only single instance can be started at once.
        /// </summary>
        public void StartProcessingCommands()
        {
            try
            {
                if (CanStartProcessing())
                    ProcessCommandsContinously();
            }
            catch (Exception e)
            {
                if (!IsStartProcessingCommandProperlyCancelled(e))
                    throw;
            }
            finally
            {
                UnlockProcessing();
            }
        }

        public void StartProcessingCommandsTillBufferEnds()
        {
            try
            {
                if (CanStartProcessing())
                    ProcessCommandsTillSomethingInBuffer();
            }
            catch (Exception e) when (IsStartProcessingCommandProperlyCancelled(e)) { }
            finally
            {
                UnlockProcessing();
            }
        }

        private bool CanStartProcessing()
        {
            lock (_processingLock)
                return _isProcessing
                    ? false
                    : _isProcessing = true;
        }

        private void UnlockProcessing()
        {
            lock (_processingLock)
                _isProcessing = false;
        }

        private void ProcessCommandsContinously()
        {
            while (true)
            {
                ICommandModel command = GetNextCommand();
                Process(command);
                _cancelProcessingCTS.Token.ThrowIfCancellationRequested();
            }
        }

        private void ProcessCommandsTillSomethingInBuffer()
        {
            while (_awaitingQueue.Count > 0)
            {
                ICommandModel command = GetNextCommand();
                Process(command);
                _cancelProcessingCTS.Token.ThrowIfCancellationRequested();
            }
        }

        private ICommandModel GetNextCommand() =>
            _awaitingQueue.Take(_cancelProcessingCTS.Token)
            ?? throw new NullReferenceException("GetNextCommand returned NULL - it really shouldn't");

        /// <summary>
        /// Process single command by trying to inject it into existing / new message and checking this message for completition / timeout.<br/>
        /// Using this method does not guarantee first priority of processing if <see cref="StartProcessingCommands"/> is running.
        /// </summary>
        /// <param name="command">Command to be processed</param>
        public void Process(ICommandModel command)
        {
            if (IsMessageAlreadyComplete(command.Id))
                HandleBadCommand(command);
            else
            {
                IMessageModel message = GetMessageToWorkWithBasedOn(command);
                if (!message?.Add(command) ?? true)
                    HandleBadCommand(command);

                if (IsComplete(message))
                    PushFromIncompleteToProcessed(message);
            }
        }

        private bool IsMessageAlreadyComplete(string messageId) => _completedMessages.ContainsKey(messageId);

        private void HandleBadCommand(ICommandModel command) => _errCommands.Add(command);

        private IMessageModel GetMessageToWorkWithBasedOn(ICommandModel command)
        {
            lock (_processingLock)
                return _incompleteMessages.GetOrAdd(command.Id, CreateNewIncomingMessage, command.TimeStamp);
        }

        private bool IsComplete(IMessageModel message) =>
            _completness.IsComplete(message);

        private IMessageModel CreateNewIncomingMessage(string id, long timestamp)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be empty", nameof(id));

            IMessageModel message = _msgFactory.CreateNew(direction: Enums.Direction.Incoming, id: id, timestampOverride: timestamp);
            _idGenerator.MarkAsUsedUp(id);
            return message;
        }

        public void PushFromIncompleteToProcessed(IMessageModel message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));
            lock (_processingLock)
                if (_incompleteMessages.TryRemove(message.Id, out IMessageModel transferedMessage))
                    if (_completedMessages.TryAdd(transferedMessage.Id, transferedMessage))
                        _processedMessages.Add(message);
        }

        private bool IsStartProcessingCommandProperlyCancelled(Exception e)
        {
            lock (_processingLock)
                _isProcessing = false;
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

        /// <summary>
        /// Gives a thread safe collection of completed and timeout messages for further processing.<br/>
        /// New messages are added as long as Ingester gets new CommandModels and can create new Messages.
        /// </summary>
        /// <returns></returns>
        public BlockingCollection<IMessageModel> GetReceivedMessages() => _processedMessages;

        /// <summary>
        /// Allows switch of source collection for easier data pipelining or producer - consumer design.
        /// </summary>
        /// <param name="newSourceCollection">New source collection</param>
        public void SwitchSourceCollectionTo(BlockingCollection<ICommandModel> newSourceCollection) =>
            _awaitingQueue = newSourceCollection ?? throw new ArgumentNullException(nameof(newSourceCollection));

        /// <summary>
        /// Returns incomplete messages till method execution from internal collection.
        /// </summary>
        /// <returns>New <see cref="Dictionary{string, IMessageModel}"/> containing incomplete messages</returns>
        public ConcurrentDictionary<string, IMessageModel> GetIncompletedMessages() => _incompleteMessages;

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
                    _cancelProcessingCTS?.Dispose();
                    _awaitingQueue?.Dispose();
                    _processedMessages?.Dispose();
                    _errCommands?.Dispose();
                    _messageProcessingLock?.Dispose();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }

        #endregion IDisposable implementation
    }
}