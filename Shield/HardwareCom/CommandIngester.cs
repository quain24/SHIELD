using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Shield.HardwareCom
{
    public class CommandIngester : ICommandIngester
    {
        private readonly Func<IMessageHWComModel> _msgFactory;
        private ICompleteness _completness;

        private readonly Dictionary<string, IMessageHWComModel> _incompleteMessages;
        private readonly BlockingCollection<ICommandModel> _awaitingQueue = new BlockingCollection<ICommandModel>();
        private readonly BlockingCollection<IMessageHWComModel> _completedMessages = new BlockingCollection<IMessageHWComModel>();
        private readonly BlockingCollection<ICommandModel> _errorAlreadyCompletedMessages = new BlockingCollection<ICommandModel>();

        private object _lock = new object();
        private bool _isProcessing = false;
        private CancellationTokenSource _cancelProcessingCTS = new CancellationTokenSource();

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
        /// <param name="completness">State check - checks if message is completed</param>
        public CommandIngester(Func<IMessageHWComModel> messageFactory, ICompleteness completness)
        {
            _msgFactory = messageFactory;
            _completness = completness;
        }

        /// <summary>
        /// Tries to ingest command into a message in internal collection if this message is not completed
        /// or there is no such message - in which case a new message is created and added to collection with this given command.
        /// </summary>
        /// <param name="incomingCommand">Command to be ingested</param>
        /// <param name="message">Message that the command was ingested into</param>
        /// <returns>True if command was ingested, false if a corresponding message was already completed</returns>
        public bool TryIngest(ICommandModel incomingCommand, out IMessageHWComModel message)
        {
            if (_incompleteMessages is null)
                throw new Exception("CommandIngester: TryIngest - There is no collection set for command to be ingested into");
            if (_completness is null)
                throw new Exception("CommandIngester: TryIngest - Completeness checker is NULL");

            message = null;

            if (incomingCommand is null)
                return false;

            if (_incompleteMessages.ContainsKey(incomingCommand.Id))
            {
                message = _incompleteMessages[incomingCommand.Id];
            }
            else
            {
                message = _msgFactory();
                message.Timestamp = Helpers.Timestamp.TimestampNow;
                message.AssaignID(incomingCommand.Id);
                message.Direction = Enums.Direction.Incoming;
                _incompleteMessages.Add(message.Id, message);
            }

            if (message is null || _completness.IsComplete(message))
                return false;

            message.Add(incomingCommand);
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
                    return;
                else
                    _isProcessing = true;
            }

            ICommandModel command;
            IMessageHWComModel message;

            while (true)
            {
                try
                {
                    command = _awaitingQueue.Take(_cancelProcessingCTS.Token);
                }
                catch
                {
                    break;
                }

                bool isIngested = TryIngest(command, out message);

                if (isIngested && _completness.IsComplete(message))
                    _completedMessages.Add(message);

                if (isIngested == false)
                    _errorAlreadyCompletedMessages.Add(command);
            }

            lock (_lock)
            {
                _isProcessing = false;
            }
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
        /// Gives a thread safe collection of completed messages for further processing
        /// </summary>
        /// <returns></returns>
        public BlockingCollection<IMessageHWComModel> GetCompletedMessages()
        {
            return _completedMessages;
        }

        /// <summary>
        /// Gives a thread safe collection of commands that were received, but corresponding messages were already completed
        /// </summary>
        /// <returns></returns>
        public BlockingCollection<ICommandModel> GetCommandsWhereMessageAlreadyCompleted()
        {
            return _errorAlreadyCompletedMessages;
        }
    }
}