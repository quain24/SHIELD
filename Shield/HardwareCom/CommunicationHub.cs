using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class CommunicationHub
    {
        // TODO
        // Do we need collections here? will we use it?
        // input collection can be located in ingester object

        private bool _hasCommunicationManager = false;

        private bool _processing = false;
        private object _processingLock = new object();

        private CancellationTokenSource _incomingCommandProcessingCTS = new CancellationTokenSource();

        private IMessanger _messanger;
        private IMessageInfoAndErrorChecks _messageInfoError;
        private ICommandIngester _commandIngester;

        // main message collections
        private ConcurrentQueue<IMessageHWComModel> _sendingQueue = new ConcurrentQueue<IMessageHWComModel>();

        private Dictionary<string, IMessageHWComModel> _sentMessages = new Dictionary<string, IMessageHWComModel>();

        private Dictionary<string, IMessageHWComModel> _receivedMessages = new Dictionary<string, IMessageHWComModel>();
        private ConcurrentQueue<ICommandModel> _commandsToProcess = new ConcurrentQueue<ICommandModel>();

        public bool DeviceIsOpen { get { return _hasCommunicationManager ? _messanger.IsOpen : false; } }
        public bool DeviceIsSending { get { return _hasCommunicationManager ? _messanger.IsSending : false; } }
        public bool DeviceIsReceiving { get { return _hasCommunicationManager ? _messanger.IsReceiving : false; } }

        #region Events

        /// <summary>
        /// Single command has been received
        /// </summary>
        public event EventHandler<CommandEventArgs> NewCommandReceived;

        /// <summary>
        /// Confirmation message has been received (proper and completed)
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> IncomingConfirmationReceived;

        /// <summary>
        /// Master message has been received (proper and completed)
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> IncomingMasterReceived;

        /// <summary>
        /// Slave message has been received (proper and completed)
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> IncomingSlaveReceived;

        /// <summary>
        /// Message containing error(s) has been received
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> IncomingErrorReceived;

        /// <summary>
        /// ERROR - This message has not been completed in specified time
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> IncomingCompletitionTimeoutOccured;

        /// <summary>
        /// Master message has been sent - awaiting confirmation from recipient
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> OutgoingMasterSent;

        /// <summary>
        /// Slave message has been sent - awaiting confirmation from recipient
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> OutgoingSlaveSent;

        /// <summary>
        /// Confirmation of received message has been sent
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> OutgoingConfirmationSent;

        /// <summary>
        /// Message has been confirmed by recipient
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> OutgoingMessageConfirmed;

        /// <summary>
        /// ERROR - This message has not been confirmed in specified time
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> OutgoingConfirmationTimeoutOccured;

        /// <summary>
        /// Outgoing message contained / triggered an error(s)
        /// </summary>
        public event EventHandler<MessageHWComEventArgs> OutgoingErrorOccured;

        #endregion Events

        public CommunicationHub(IMessageInfoAndErrorChecks messageInfoError,
                                ICommandIngester commandIngester)
        {
            _messageInfoError = messageInfoError;
            _commandIngester = commandIngester;
        }

        #region Setup methods

        public long ConfirmationTimeout(long timeout = -1)
        {
            if (timeout < 0 || timeout % 1 != 0)
                return _messageInfoError?.ConfirmationTimeout ?? -1;

            return _messageInfoError is null ? -1 : _messageInfoError.ConfirmationTimeout = timeout;
        }

        public long CompletitionTimeout(long timeout = -1)
        {
            if (timeout < 0 || timeout % 1 != 0)
                return _messageInfoError?.CompletitionTimeout ?? -1;

            return _messageInfoError is null ? -1 : _messageInfoError.CompletitionTimeout = timeout;
        }

        public void AssignMessanger(IMessanger messanger)
        {
            if (messanger is null)
                return;
            if (_hasCommunicationManager && _messanger != null)
                _messanger.CommandReceived -= OnCommandReceivedinternally;

            _messanger = messanger;
            _messanger.CommandReceived += OnCommandReceivedinternally;
            _hasCommunicationManager = true;
        }

        #endregion Setup methods

        public async Task SendMessage(IMessageHWComModel message)
        {
        }

        private void AddToProcessingQueue(ICommandModel command)
        {
            _commandsToProcess.Enqueue(command);
        }

        private void TryAddCommandToMessage(ICommandModel command)
        { // single thread - one command at a time
            IMessageHWComModel message;

            if (_receivedMessages.ContainsKey(command.Id))
            {
                message = _receivedMessages[command.Id];
                if (_messageInfoError.IsCompleted(message))
                {
                    // ERROR - corresponding message is already completed!
                    // handle, give to other, exit?
                }
                if (_messageInfoError.IsCompletitionTimeoutExceeded(message))
                {
                    message.Errors = message.Errors | Enums.Errors.CompletitionTimeout;
                    // ERROR - corresponding message hit completition timeout
                    // handle, give to other, exit?
                }
            }
            else if (_commandIngester.TryIngest(command, out message))
            {
                if (_messageInfoError.IsCompleted(message))
                {
                    // GOOD - completed, give it to handler
                }
                else
                {
                    // NEUTRAL - not completed, but also still in time - so just wait for another command
                    //           or for completeness timeout checker from other thread?
                }
            }

            // Inject command into incomplete message
            if (_messageInfoError.IsCompletitionTimeoutExceeded(message) == false)
            {
                // Completition timeout is not exceeded, so check if this message is complete
                if (_messageInfoError.IsCompleted(message))
                {
                    // Message is complete, its isComplete is set, now time to check for errors
                    message.Errors = message.Errors | _messageInfoError.DecodingErrorsIn(message);
                    if (_messageInfoError.IsPatternCorrect(message) == false)
                        message.Errors = message.Errors | Enums.Errors.BadMessagePattern;
                    message.Type = _messageInfoError.DetectTypeOf(message);
                    if (message.Type == Enums.MessageType.Unknown)
                        message.Errors = message.Errors | Enums.Errors.UndeterminedType;
                }
                else
                {
                    // incomplete but there is still time for it to be completed
                    // what to do? Nothing?
                }
            }
            else
            {
                // not ingested - so completed, cannot add another command - trash it
            }
        }

        private bool IsMsgAlreadyCompleted(string id)
        {
            return (_receivedMessages.ContainsKey(id) && _receivedMessages[id].IsCompleted);
        }

        #region Event handlers

        // Command received from IMessanger
        protected virtual void OnCommandReceivedinternally(object sender, ICommandModel command)
        {
            command.TimeStamp = Timestamp.TimestampNow;
            IdGenerator.UsedThisID(command.Id);

            AddToProcessingQueue(command);
        }

        #endregion Event handlers
    }
}