using Shield.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class ComCommander
    {
        #region Autofac objects

        private ICommandModelFactory _commandFactory;
        private Func<IMessageModel> _messageFactory;
        private IMessenger _messanger;

        #endregion Autofac objects

        #region Locks

        private object _receiverLock = new object();
        private bool _receiverRunning = false;

        #endregion Locks

        #region Variables

        private long _confirmationTimeout = 2000;
        private long _completitionTimeout = 2000;
        private bool _hasCommunicationManager = false;

        #endregion Variables

        #region Variables - Sent messages collections

        private ConcurrentQueue<IMessageModel> _outgoingQueue = new ConcurrentQueue<IMessageModel>();
        private Dictionary<string, IMessageModel> _outgoingSent = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _outgoingConfirmed = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _outgoingConfirmations = new Dictionary<string, IMessageModel>();
        private Dictionary<string, (MessageErrors errorType, IMessageModel Message)> _outgoingErrors = new Dictionary<string, (MessageErrors errorType, IMessageModel Message)>();

        #endregion Variables - Sent messages collections

        #region Variables - Received messages collections

        private ConcurrentQueue<ICommandModel> _incomingQueue = new ConcurrentQueue<ICommandModel>();
        private Dictionary<string, IMessageModel> _incomingPartial = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingCompleteConfirms = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingCompleteSlaves = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingCompleteMasters = new Dictionary<string, IMessageModel>();
        private Dictionary<string, (MessageErrors errorType, IMessageModel Message)> _incomingErrors = new Dictionary<string, (MessageErrors errorType, IMessageModel Message)>();

        #endregion Variables - Received messages collections

        #region Events

        /// <summary>
        /// Single command has been received
        /// </summary>
        public event EventHandler<CommandEventArgs> CommandReceived;

        /// <summary>
        /// Confirmation message has been received (proper and completed)
        /// </summary>
        public event EventHandler<MessageEventArgs> IncomingConfirmationReceived;

        /// <summary>
        /// Master message has been received (proper and completed)
        /// </summary>
        public event EventHandler<MessageEventArgs> IncomingMasterReceived;

        /// <summary>
        /// Slave message has been received (proper and completed)
        /// </summary>
        public event EventHandler<MessageEventArgs> IncomingSlaveReceived;

        /// <summary>
        /// Message containing error(s) has been received
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> IncomingErrorReceived;

        /// <summary>
        /// ERROR - This message has not been completed in specified time
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> IncomingCompletitionTimeoutOccured;

        /// <summary>
        /// Message has been sent - awaiting confirmation from recipient
        /// </summary>
        public event EventHandler<MessageEventArgs> OutgoingMessageSent;

        /// <summary>
        /// Message has been confirmed by recipient
        /// </summary>
        public event EventHandler<MessageEventArgs> OutgoingMessageConfirmed;

        /// <summary>
        /// ERROR - This message has not been confirmed in specified time
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> OutgoingConfirmationTimeoutOccured;

        /// <summary>
        /// Outgoing message contained / triggered an error(s)
        /// </summary>
        public event EventHandler<MessageErrorEventArgs> OutgoingErrorOccured;

        #endregion Events

        #region Properties

        public bool DeviceIsOpen { get { return _hasCommunicationManager ? _messanger.IsOpen : false; } }

        #endregion Properties

        public ComCommander(ICommandModelFactory commandFactory, Func<IMessageModel> messageFactory)
        {
            // AutoFac auto factories
            _commandFactory = commandFactory;
            _messageFactory = messageFactory;
        }

        public void AssignMessanger(IMessenger messanger)
        {
            if (messanger is null)
                return;
            if (_hasCommunicationManager)
            {
                _messanger.CommandReceived -= OnCommandReceivedinternally;
            }

            _messanger = messanger;
            _messanger.CommandReceived += OnCommandReceivedinternally;
            _hasCommunicationManager = true;
        }

        #region Incoming messages handlers

        public void IncomingConfirmationMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            var elements = from commands in message
                           group commands by commands.CommandType into types
                           select new { types.Key, Count = types.Count() };

            int receivedErrors = 0;
            int receivedUnknowns = 0;
            int receivedPartials = 0;

            foreach (var c in elements)
            {
                receivedErrors = c.Key == CommandType.ReceivedAsError ? c.Count : receivedErrors;
                receivedUnknowns = c.Key == CommandType.ReceivedAsUnknown ? c.Count : receivedUnknowns;
                receivedPartials = c.Key == CommandType.ReceivedAsPartial ? c.Count : receivedPartials;
            }

            // Everything is good, message was properly received
            if (receivedErrors == 0 && receivedUnknowns == 0 && receivedPartials == 0)
            {
                if (SwitchBuffers(message, _outgoingSent, _outgoingConfirmed))
                {
                    OutgoingConfirmedMessagehandler(message);
                }
                else
                {
                    throw new InvalidOperationException("Could not transfer message from Sent to Confirmed buffer.");
                }
                return;
            }

            // Something went bad - sent message was received with errors.
            // Pushing message that was confirmed to error buffer (at the end) and removing from sent buffer

            IMessageModel tmpErrMessage = _outgoingSent[message.Id];
            MessageErrors error = MessageErrors.None;

            // Error checking Conditions and insert into outgoing error buffer

            if (receivedUnknowns > 0)
                error = error | MessageErrors.GotUnknownCommands;

            if (receivedPartials > 0)
                error = error | MessageErrors.GotPartialCommands;

            if (receivedErrors > 0)
                error = error | MessageErrors.GotErrorCommands;

            SwitchBuffers(tmpErrMessage, error, _outgoingSent, _outgoingErrors);
            OutgoingErrorrousMessageHandler(tmpErrMessage);

            return;
        }

        public void IncomingMasterMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            AddToSendingQueue(CreateConfirmationOf(message));
            SendNextQueuedMessageAsync();

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public void IncomingSlaveMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            AddToSendingQueue(CreateConfirmationOf(message));
            SendNextQueuedMessageAsync();

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public void IncomingErrorrousMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            // Will act according to given error type, for now just respond and display.
            AddToSendingQueue(CreateConfirmationOf(message));
            SendNextQueuedMessageAsync();

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            Console.WriteLine($@"ERROR TYPE: {_incomingErrors[message.Id].errorType}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        #endregion Incoming messages handlers

        #region Outgoing message handlers

        public void OutgoingErrorrousMessageHandler(IMessageModel message)
        {
            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public void OutgoingConfirmedMessagehandler(IMessageModel message)
        {
            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        private IMessageModel CreateConfirmationOf(IMessageModel message)
        {
            if (message is null || message.ElementAt(1).CommandType == CommandType.Confirmation)
                return null;

            IMessageModel confirmation = _messageFactory();
            confirmation.Add(_commandFactory.Create(CommandType.HandShake));
            confirmation.Add(_commandFactory.Create(CommandType.Confirmation));

            foreach (var c in message)
            {
                ICommandModel responseCommand = _commandFactory.Create();
                switch (c.CommandType)
                {
                    case CommandType.Error:
                        responseCommand.CommandType = CommandType.ReceivedAsError;
                        break;

                    case CommandType.Unknown:
                        responseCommand.CommandType = CommandType.ReceivedAsUnknown;
                        break;

                    case CommandType.Partial:
                        responseCommand.CommandType = CommandType.ReceivedAsPartial;
                        break;

                    default:
                        responseCommand.CommandType = CommandType.ReceivedAsCorrect;
                        break;
                }
                confirmation.Add(responseCommand);
            }
            confirmation.Add(_commandFactory.Create(CommandType.EndMessage));
            confirmation.AssaignID(message.Id);
            return confirmation;
        }

        #endregion Outgoing message handlers

        #region Conductors

        public void IncomingConductor(IMessageModel message)
        {
            if (message is null)
                return;

            // Check if message is complete
            if (IsCompleted(message))
            {
                // Check for Possible errors - if bad, then off to incoming error handler
                MessageErrors decodingErrors = CheckIfDecodedCorrectly(message);
                bool patternCorrect = IsPatternCorrect(message);
                IncomingType messageType = MessageType(message);

                if (patternCorrect == false)
                    decodingErrors = decodingErrors | MessageErrors.BadMessagePattern;
                if (messageType == IncomingType.Undetermined)
                    decodingErrors = decodingErrors | MessageErrors.UndeterminedType;

                if (decodingErrors != MessageErrors.None)
                {
                    SwitchBuffers(message, decodingErrors, _incomingPartial, _incomingErrors);
                    IncomingErrorrousMessageHandler(message);
                    return;
                }

                // Else, message is completed, decoded properly - now off to corresponding buffers and handlers
                switch (messageType)
                {
                    case IncomingType.Confirmation:
                        if (!_outgoingSent.ContainsKey(message.Id))
                        {
                            SwitchBuffers(message, MessageErrors.ConfirmedNonexistent, _incomingPartial, _incomingErrors);
                            IncomingErrorrousMessageHandler(message);
                            break;
                        }
                        SwitchBuffers(message, _incomingPartial, _incomingCompleteConfirms);
                        IncomingConfirmationMessageHandler(message);
                        break;

                    case IncomingType.Master:
                        SwitchBuffers(message, _incomingPartial, _incomingCompleteMasters);
                        IncomingMasterMessageHandler(message);
                        break;

                    case IncomingType.Slave:
                        if (!_outgoingConfirmed.ContainsKey(message.Id))
                        {
                            SwitchBuffers(message, MessageErrors.RespondedToNonexistent, _incomingPartial, _incomingErrors);
                            IncomingErrorrousMessageHandler(message);
                            break;
                        }
                        SwitchBuffers(message, _incomingPartial, _incomingCompleteSlaves);
                        IncomingSlaveMessageHandler(message);
                        break;

                    // Should not happen ever, just for sanity
                    default:
                        decodingErrors = MessageErrors.UndeterminedType;
                        SwitchBuffers(message, decodingErrors, _incomingPartial, _incomingErrors);
                        IncomingErrorrousMessageHandler(message);
                        break;
                }
            }
            // Message incomplete
            else
            {
                // Nothing, wait for another command
                return;
            }
        }

        #endregion Conductors

        #region Message Sending

        public bool AddToSendingQueue(IMessageModel message)
        {
            if (message is null)
                return false;

            message.Timestamp = Timestamp.TimestampNow;
            _outgoingQueue.Enqueue(message);
            return true;
        }

        public async Task<bool> SendNextQueuedMessageAsync()
        {
            if (DeviceIsOpen == false)
                return false;

            IMessageModel message;
            bool wasSent = false;

            if (_outgoingQueue.TryDequeue(out message))
            {
                wasSent = await _messanger.SendAsync(message).ConfigureAwait(false);
                message.Timestamp = Timestamp.TimestampNow;
                if (wasSent)
                {
                    if (message.ElementAt(1).CommandType == CommandType.Confirmation)
                    {
                        _outgoingConfirmations[message.Id] = message;
                    }
                    _outgoingSent[message.Id] = message;
                }
                else
                {
                    _outgoingErrors[message.Id] = (MessageErrors.NotSent, message);
                }
            }
            return wasSent;
        }

        public async Task<bool> SendQueuedMessages(CancellationToken ct)
        {
            if (_outgoingQueue.Count == 0)
                return false;

            while (_outgoingQueue.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                await SendNextQueuedMessageAsync().ConfigureAwait(false);
                ct.ThrowIfCancellationRequested();
            }
            return true;
        }

        #endregion Message Sending

        private void Receiver()
        {
            if (!_receiverRunning)
            {
                lock (_receiverLock)
                {
                    if (!_receiverRunning) // If first check was hit in the same time twice - safety net
                        _receiverRunning = true;
                    else
                        return;
                }
            }

            ICommandModel command;
            while (_incomingQueue.TryDequeue(out command))
            {
                Buffer commandDepositedIn = ChooseBufferFor(command);
                string messageId = command.Id;

                if (commandDepositedIn == Buffer.IncomingPartial)
                {
                    IncomingConductor(_incomingPartial[messageId]);
                }
            }

            lock (_receiverLock)
            {
                _receiverRunning = false;
            }
        }

        #region Error, state and type checking

        private Buffer ChooseBufferFor(ICommandModel command)
        {
            if (command is null)
                return Buffer.None;

            if (_incomingErrors.ContainsKey(command.Id))
            {
                _incomingErrors[command.Id].Message.Add(command);
                return Buffer.IncomingError;
            }

            if (_incomingPartial.ContainsKey(command.Id))
            {
                _incomingPartial[command.Id].Add(command);
                return Buffer.IncomingPartial;
            }
            else
            {
                IMessageModel newMessage = _messageFactory();
                newMessage.Timestamp = Timestamp.TimestampNow;
                newMessage.AssaignID(command.Id);
                newMessage.Add(command);
                _incomingPartial[newMessage.Id] = newMessage;
            }
            return Buffer.IncomingPartial;
        }

        private static MessageErrors CheckIfDecodedCorrectly(IMessageModel message)
        {
            if (message is null)
                return MessageErrors.IsNull;

            List<ICommandModel> badOrUnknown = message
                .Where(c =>
                    c.CommandType == CommandType.Unknown ||
                    c.CommandType == CommandType.Error ||
                    c.CommandType == CommandType.Partial)
                .ToList();

            if (!badOrUnknown.Any())
                return MessageErrors.None;

            int numberOfUnknowns = 0;
            int numberOfErrors = 0;
            int numberOfPartials = 0;

            MessageErrors errors = MessageErrors.None;

            foreach (ICommandModel c in badOrUnknown)
            {
                if (c.CommandType == CommandType.Error)
                    numberOfErrors++;
                else if (c.CommandType == CommandType.Unknown)
                    numberOfUnknowns++;
                else if (c.CommandType == CommandType.Partial)
                    numberOfPartials++;
            }

            if (numberOfErrors > 0)
                errors = errors | MessageErrors.GotErrorCommands;
            if (numberOfPartials > 0)
                errors = errors | MessageErrors.GotPartialCommands;
            if (numberOfUnknowns > 0)
                errors = errors | MessageErrors.GotUnknownCommands;

            return errors;
        }

        private static bool IsCompleted(IMessageModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "Cannot pass null message!");

            if (message.IsTransmissionCompleted)
                return true;

            if (message.Last().CommandType == CommandType.EndMessage)
            {
                message.IsTransmissionCompleted = true;
                return true;
            }

            return false;
        }

        private static bool IsPatternCorrect(IMessageModel message)
        {
            // not enough commands in message
            if (message.Count() < 3)
                return false;

            // Correct beginning and end?
            if (message.First().CommandType != CommandType.HandShake ||
                message.Last().CommandType != CommandType.EndMessage)
                return false;

            // Only one begin and one end?
            if (message.Count(c => c.CommandType == CommandType.HandShake || c.CommandType == CommandType.EndMessage) != 2)
                return false;

            // Message type in correct place?
            CommandType messageType = message.ElementAt(1).CommandType;
            if (messageType != CommandType.Master && messageType != CommandType.Slave && messageType != CommandType.Confirmation)
            {
                return false;
            }

            // Only one message type?
            if (message.
                Count(c =>
                    c.CommandType == CommandType.Master ||
                    c.CommandType == CommandType.Slave ||
                    c.CommandType == CommandType.Confirmation)
                != 1)
            {
                return false;
            }

            return true;
        }

        private static IncomingType MessageType(IMessageModel message)
        {
            CommandType type = message.ElementAt(1).CommandType;

            switch (type)
            {
                case CommandType.Master:
                    return IncomingType.Master;

                case CommandType.Slave:
                    return IncomingType.Slave;

                case CommandType.Confirmation:
                    return IncomingType.Confirmation;

                default:
                    return IncomingType.Undetermined;
            }
        }

        #endregion Error, state and type checking

        #region Internal helpers

        private bool InConfirmationWindow(IMessageModel message)
        {
            if (_outgoingSent.ContainsKey(message.Id) &&
                Timestamp.Difference(_outgoingSent[message.Id].Timestamp) <= _confirmationTimeout)
            {
                return true;
            }
            return false;
        }

        private bool InCompletitionWindow(IMessageModel message)
        {
            if (_incomingPartial.ContainsKey(message.Id) &&
                Timestamp.Difference(_incomingPartial[message.Id].Timestamp) <= _completitionTimeout)
            {
                return true;
            }
            return false;
        }

        public long ConfirmationTimeout(long timeout)
        {
            if (timeout < 0 || timeout % 1 != 0)
                return _confirmationTimeout;

            _confirmationTimeout = timeout;
            return _confirmationTimeout;
        }

        public long ConfirmationTimeout()
        {
            return _confirmationTimeout;
        }

        public long CompletitionTimeout(long timeout)
        {
            if (timeout < 0 || timeout % 1 != 0)
                return _confirmationTimeout;

            _confirmationTimeout = timeout;
            return _confirmationTimeout;
        }

        public long CompletitionTimeout()
        {
            return _completitionTimeout;
        }

        public List<IMessageModel> CompletitionTimeoutCheckAndClear()
        {
            List<IMessageModel> timeoutedMessages = new List<IMessageModel>();

            timeoutedMessages = _incomingPartial
                .Where(kvp => InCompletitionWindow(kvp.Value) == false)
                .Select(kvp => kvp.Value)
                .ToList();

            timeoutedMessages.ForEach(message =>
            {
                MessageErrors decodingErrors = CheckIfDecodedCorrectly(message);
                bool patternCorrect = IsPatternCorrect(message);
                IncomingType messageType = MessageType(message);

                if (patternCorrect == false)
                    decodingErrors = decodingErrors | MessageErrors.BadMessagePattern;
                if (messageType == IncomingType.Undetermined)
                    decodingErrors = decodingErrors | MessageErrors.UndeterminedType;
                decodingErrors = decodingErrors | MessageErrors.Incomplete | MessageErrors.CompletitionTimeout;

                SwitchBuffers(message, decodingErrors, _incomingPartial, _incomingErrors);
            });

            return timeoutedMessages;
        }

        public List<IMessageModel> ConfirmationTimeoutCheckAndClean()
        {
            List<IMessageModel> unconfirmedMessages = new List<IMessageModel>();

            unconfirmedMessages = _outgoingSent
                .Where(kvp => InConfirmationWindow(kvp.Value) == false)
                .Select(kvp => kvp.Value)
                .ToList();

            unconfirmedMessages.ForEach(message =>
            {
                SwitchBuffers(message, MessageErrors.ConfirmationTimeout, _outgoingSent, _outgoingErrors);
            });

            return unconfirmedMessages;
        }

        #endregion Internal helpers

        #region Event handlers

        protected virtual void OnCommandReceivedinternally(object sender, ICommandModel command)
        {
            command.TimeStamp = Timestamp.TimestampNow;
            IdGenerator.UsedThisID(command.Id);
            _incomingQueue.Enqueue(command);
            Receiver();
        }

        protected virtual void OnCommandReceived(object sender, CommandEventArgs e)
        {
            CommandReceived?.Invoke(sender, e);
        }

        protected virtual void OnIncomingConfirmation(object sender, MessageEventArgs e)
        {
            IncomingConfirmationReceived?.Invoke(sender, e);
        }

        protected virtual void OnIncomingMasterReceived(object sender, MessageEventArgs e)
        {
            IncomingMasterReceived?.Invoke(sender, e);
        }

        protected virtual void OnIncomingSlaveReceived(object sender, MessageEventArgs e)
        {
            IncomingSlaveReceived?.Invoke(sender, e);
        }

        protected virtual void OnIncomingErrorReceived(object sender, MessageErrorEventArgs e)
        {
            IncomingErrorReceived?.Invoke(sender, e);
        }

        protected virtual void OnIncomingCompletitionTimeoutOccured(object sender, MessageErrorEventArgs e)
        {
            IncomingCompletitionTimeoutOccured?.Invoke(sender, e);
        }

        protected virtual void OnOutgoingMessageSent(object sender, MessageEventArgs e)
        {
            OutgoingMessageSent?.Invoke(sender, e);
        }

        protected virtual void OnOutgoingMessageConfirmed(object sender, MessageEventArgs e)
        {
            OutgoingMessageConfirmed?.Invoke(sender, e);
        }

        protected virtual void OnOutgoingErrorOccured(object sender, MessageErrorEventArgs e)
        {
            OutgoingErrorOccured?.Invoke(sender, e);
        }

        protected virtual void OnOutgoingConfirmationTimeoutOccured(object sender, MessageErrorEventArgs e)
        {
            OutgoingConfirmationTimeoutOccured?.Invoke(sender, e);
        }

        #endregion Event handlers

        #region Internal message transfer between buffers

        private static bool SwitchBuffers(IMessageModel message,
                                   Dictionary<string, IMessageModel> source,
                                   Dictionary<string, IMessageModel> target)
        {
            if (message is null || source is null || target is null)
                return false;

            if (source == target)
                return false;

            if (!source.ContainsKey(message.Id))
                return false;

            target[message.Id] = message;
            source.Remove(message.Id);
            return true;
        }

        private static bool SwitchBuffers(IMessageModel message,
                                   MessageErrors error,
                                   Dictionary<string, IMessageModel> source,
                                   Dictionary<string, (MessageErrors errorType, IMessageModel Message)> target)
        {
            if (message is null || source is null ||
                target is null || !source.ContainsKey(message.Id))
                return false;

            target[message.Id] = (error, message);
            source.Remove(message.Id);
            return true;
        }

        private static bool SwitchBuffers(IMessageModel message,
                                   Dictionary<string, (MessageErrors errorType, IMessageModel Message)> source,
                                   Dictionary<string, IMessageModel> target)
        {
            if (message is null || source is null ||
                target is null || !source.ContainsKey(message.Id))
                return false;

            target[message.Id] = message;
            source.Remove(message.Id);
            return true;
        }

        #endregion Internal message transfer between buffers

        #region Internal Enums

        internal enum Buffer
        {
            None,
            IncomingPartial,
            IncomingError
        }

        internal enum IncomingType
        {
            Master,
            Slave,
            Confirmation,
            Undetermined
        }

        #endregion Internal Enums
    }
}