using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class CommandIngester : ICommandIngester
    {
        private readonly IMessageFactory _msgFactory;
        private ICompleteness _completness;
        private ITimeoutCheck _completitionTimeout;

        private readonly BlockingCollection<ICommandModel> _awaitingQueue = new BlockingCollection<ICommandModel>();

        private readonly Dictionary<string, IMessageModel> _incompleteMessages =
            new Dictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);

        private readonly BlockingCollection<IMessageModel> _processedMessages = new BlockingCollection<IMessageModel>();
        private readonly BlockingCollection<ICommandModel> _errCommands = new BlockingCollection<ICommandModel>();

        private object _lock = new object();
        private object _timeoutCheckLock = new object();
        private bool _isProcessing = false;
        private bool _isTimeoutChecking = false;
        private CancellationTokenSource _cancelProcessingCTS = new CancellationTokenSource();
        private CancellationTokenSource _cancelTimeoutCheckCTS = new CancellationTokenSource();

        /// <summary>
        /// Returns true if commands are being processed or object waits for new commands to be processed
        /// </summary>
        public bool IsProcessingCommands
        {
            get => _isProcessing;
        }

        /// <summary>
        /// Will ingest given commands into corresponding messages or will create new messages for those commands.
        /// If commands is to be ingested into already completed message - it will be put in separate collection.
        /// </summary>
        /// <param name="messageFactory">Message factory delegate</param>
        /// <param name="completeness">State check - checks if message is completed</param>
        /// <param name="completitionTimeout">State check - optional - checks if completition time is exceeded</param>
        public CommandIngester(IMessageFactory messageFactory, ICompleteness completeness, ITimeoutCheck completitionTimeout = null)
        {
            _msgFactory = messageFactory;
            _completness = completeness;
            _completitionTimeout = completitionTimeout;
        }

        /// <summary>
        /// Tries to ingest command into a message in internal collection if this message is not completed
        /// or there is no such message - in which case a new message is created and added to collection with this given command.
        /// </summary>
        /// <param name="incomingCommand">Command to be ingested</param>
        /// <param name="message">Message that the command was ingested into</param>
        /// <returns>True if command was ingested, false if a corresponding message was already completed</returns>
        public bool TryIngest(ICommandModel incomingCommand, out IMessageModel message)
        {
            if (_completness is null)
                throw new Exception("CommandIngester: TryIngest - Completeness checker is NULL");
            if (incomingCommand is null)
                throw new ArgumentNullException(nameof(incomingCommand), "CommandIngester: TryIngest - tried to ingest NULL");

            Console.WriteLine($@"CommandIngester TryIngest command {incomingCommand.Id}");

            // In any case, add command id to used-up pool on this machine
            Helpers.IdGenerator.UsedThisID(incomingCommand.Id);

            if (_incompleteMessages.ContainsKey(incomingCommand.Id))
            {
                message = _incompleteMessages[incomingCommand.Id];
                Console.WriteLine("CommandIngester - Incomplete message with command id found!");
            }
            else
            {
                message = _msgFactory.CreateNew(direction: Enums.Direction.Incoming, id: incomingCommand.Id);
                _incompleteMessages.Add(message.Id, message);
                Console.WriteLine($@"CommandIngester - new message created");
            }

            // check for old completes or removed otherwise
            if (message is null || _completness.IsComplete(message))
            {
                _errCommands.Add(incomingCommand);
                Console.WriteLine("CommandIngester - ERROR - tried to add new command to completed message");
                return false;
            }

            // check for completition timeout if checker exists
            if (_completitionTimeout?.IsExceeded(message) ?? false)
            {
                message.Errors |= Enums.Errors.CompletitionTimeout;
                _processedMessages.Add(message);
                _errCommands.Add(incomingCommand);
                _incompleteMessages[incomingCommand.Id] = null;
                Console.WriteLine("CommandIngester - Error - message  completition timeoutted");
                return false;
            }

            message.Add(incomingCommand);

            // if completed after last command, then move to processed
            if (_completness.IsComplete(message))
            {
                _processedMessages.Add(message);
                _incompleteMessages[message.Id] = null;
                Console.WriteLine("CommandIngester - Message was processed, adding to processed messages collection");
            }

            return true;
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
        /// Thread safe
        /// </summary>
        public void StartProcessingCommands()
        {
            lock (_lock)
            {
                if (_isProcessing)
                {
                    Console.WriteLine("CommandIngester - commands are already being processed");
                    return;
                }
                else
                    _isProcessing = true;
            }

            Console.WriteLine("CommandIngester - Command processing started");

            ICommandModel command = null;
            IMessageModel message;
            int debugCounter = 0;
            int debugLoopCounter = 0;

            while (true)
            {
                try
                {
                    bool a = _awaitingQueue.TryTake(out command, 150, _cancelProcessingCTS.Token);

                    if (a) Console.WriteLine($"CommandIngester - Took command {command.Id} for processing");
                    else
                    {
                        if (++debugLoopCounter % 10 == 0) Console.WriteLine($@"CommandIngester - Could not take command for processing: tried {++debugCounter} times, {_awaitingQueue.Count} available.");
                    }
                }
                catch
                {
                    break;
                }
                if (command != null)
                    TryIngest(command, out message);
            }
            Console.WriteLine("CommandIngester - StartProcessingCommands ENDED");
            _isProcessing = false;
        }

        /// <summary>
        /// Stops processing commands
        /// </summary>
        public void StopProcessingCommands()
        {
            _cancelProcessingCTS.Cancel();
            _cancelProcessingCTS = new CancellationTokenSource();
        }

        public async Task StartTimeoutCheck(int interval = 0)
        {
            if (_completitionTimeout is null)
                return;

            lock (_timeoutCheckLock)
            {
                if (!_isTimeoutChecking)
                    _isTimeoutChecking = true;
                else
                    return;
            }

            int checkInterval = CalcCheckInterval(interval);

            while (true)
            {
                try
                {
                    _cancelTimeoutCheckCTS.Token.ThrowIfCancellationRequested();

                    var timeouts = _completitionTimeout?.GetTimeoutsFromCollection(GetIncompletedMessages());

                    foreach (var message in timeouts)
                    {
                        message.Errors |= Enums.Errors.CompletitionTimeout;
                        _processedMessages.Add(message);
                        _incompleteMessages[message.Id] = null;
                        Console.WriteLine("ContinousTimeoutChecker - Error - message  completition timeoutted");
                        _cancelTimeoutCheckCTS.Token.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(checkInterval, _cancelTimeoutCheckCTS.Token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (e is TaskCanceledException || e is OperationCanceledException)
                        _isTimeoutChecking = false;
                    else
                        throw;
                }
            }
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

        public Dictionary<string, IMessageModel> GetIncompletedMessages()
        {
            return _incompleteMessages;
        }

        /// <summary>
        /// Gives a thread safe collection of commands that were received, but corresponding messages were already completed or completition timeout
        /// was exceeded.
        /// </summary>
        /// <returns></returns>
        public BlockingCollection<ICommandModel> GetErrAlreadyCompleteOrTimeout()
        {
            return _errCommands;
        }

        private int CalcCheckInterval(long timeout)
        {
            switch (timeout)
            {
                case var _ when timeout <= _completitionTimeout.NoTimeoutValue:
                return 250;

                case var _ when timeout <= 100:
                return 10;

                case var _ when timeout <= 3000:
                return 100;

                case var _ when timeout > 3000:
                return 250;

                default:
                return _completitionTimeout.NoTimeoutValue;
            }
        }
    }
}