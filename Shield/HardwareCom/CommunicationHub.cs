using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class CommunicationHub
    {
        #region Settings and info

        private bool _hasCommunicationManager = false;
        private long _confirmationTimeout = 2000;
        private long _completitionTimeout = 2000;

        // locks
        private bool _processing = false;

        private object _processingLock = new object();

        #endregion Settings and info

        #region Objects from Autofac and settings - setters

        private IMessageInfoAndErrorChecks _msgInfoError;
        private IMessanger _messanger;
        private Func<IMessageHWComModel> _messageFactory;
        private ICommandModelFactory _commandFactory;

        #endregion Objects from Autofac and settings - setters

        // main message collections
        private Dictionary<string, IMessageHWComModel> _receivedMessages = new Dictionary<string, IMessageHWComModel>();

        private Dictionary<string, IMessageHWComModel> _sentMessages = new Dictionary<string, IMessageHWComModel>();
        private ConcurrentQueue<IMessageHWComModel> _sendingQueue = new ConcurrentQueue<IMessageHWComModel>();

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

        public CommunicationHub(IMessageInfoAndErrorChecks msgInfoError,
                                ICommandModelFactory commandFactory,
                                Func<IMessageHWComModel> messageFactory)
        {
            _msgInfoError = msgInfoError;
            _messageFactory = new Func<IMessageHWComModel>(() => { return new MessageHWComModel(); }); //_messageFactory = messageFactory;
            _commandFactory = commandFactory;
        }

        #region Setup methods

        public long ConfirmationTimeout(long timeout = -1)
        {
            if (timeout < 0 || timeout % 1 != 0)
                return _confirmationTimeout;

            _confirmationTimeout = timeout;
            _msgInfoError.ConfirmationTimeout = _confirmationTimeout;
            return _confirmationTimeout;
        }

        public long CompletitionTimeout(long timeout = -1)
        {
            if (timeout < 0 || timeout % 1 != 0)
                return _confirmationTimeout;

            _confirmationTimeout = timeout;
            _msgInfoError.CompletitionTimeout = _completitionTimeout;
            return _confirmationTimeout;
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

        private void ProcessIncomingCommand(ICommandModel command)
        {
        }

        #region Event handlers

        // Command received from IMessanger
        protected virtual void OnCommandReceivedinternally(object sender, ICommandModel command)
        {
            command.TimeStamp = Timestamp.TimestampNow;
            IdGenerator.UsedThisID(command.Id);

            ProcessIncomingCommand(command);
        }

        #endregion Event handlers
    }
}