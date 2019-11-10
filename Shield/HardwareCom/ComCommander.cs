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

// Pomyslec nad wycofaniem konfirmacji gdy dostajemy timeouta - zniknie po napisaniu wlasciwej metody error handlera
// refactoring - klasa jest zbyt wielka

namespace Shield.HardwareCom
{
    public class ComCommander
    {
        #region Autofac objects

        private ICommandModelFactory _commandFactory;
        private Func<IMessageModel> _messageFactory;
        private IMessanger _messanger;

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
        /// Master message has been sent - awaiting confirmation from recipient
        /// </summary>
        public event EventHandler<MessageEventArgs> OutgoingMasterSent;

        /// <summary>
        /// Slave message has been sent - awaiting confirmation from recipient
        /// </summary>
        public event EventHandler<MessageEventArgs> OutgoingSlaveSent;

        /// <summary>
        /// Confirmation of received message has been sent
        /// </summary>
        public event EventHandler<MessageEventArgs> OutgoingConfirmationSent;

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

        #region Settings, setups, etc

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

        public void AssignMessanger(IMessanger messanger)
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

        #endregion Settings, setups, etc

        #region Incoming messages handlers

        private void IncomingConfirmationMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            OnIncomingConfirmationReceived(this, new MessageEventArgs(message));

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

        private void IncomingMasterMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            AddToSendingQueue(CreateConfirmationOf(message));
            SendNextQueuedMessageAsync();

            OnIncomingMasterReceived(this, new MessageEventArgs(message));

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        private void IncomingSlaveMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            AddToSendingQueue(CreateConfirmationOf(message));
            SendNextQueuedMessageAsync();

            OnIncomingSlaveReceived(this, new MessageEventArgs(message));

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        private void IncomingErrorrousMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            // Will act according to given error type, for now just respond and display.
            AddToSendingQueue(CreateConfirmationOf(message));
            SendNextQueuedMessageAsync();

            // testing event sending

            OnIncomingErrorReceived(this, new MessageErrorEventArgs(message, _incomingErrors[message.Id].errorType));

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

        private void OutgoingMasterMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            OnOutgoingMasterSent(this, new MessageEventArgs(message));

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        private void OutgoingSlaveMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            OnOutgoingSlaveSent(this, new MessageEventArgs(message));

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        private void OutgoingConfirmationSend(IMessageModel message)
        {
            if (message is null)
                return;

            OnOutgoingConfirmationSent(this, new MessageEventArgs(message));

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        private void OutgoingErrorrousMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            OnOutgoingErrorOccured(this, new MessageErrorEventArgs(message, _outgoingErrors[message.Id].errorType));

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        private void OutgoingConfirmedMessagehandler(IMessageModel message)
        {
            if (message is null)
                return;

            OnOutgoingMessageConfirmed(this, new MessageEventArgs(message));

            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach (var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if (c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        #endregion Outgoing message handlers

        #region Conductors

        private void IncomingConductor(IMessageModel message)
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
                if (CompletitionTimeoutExceeded(message))
                {
                    ProcessCompletitionTimeout(message);
                }
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
                    CommandType type = message.Count() <= 1 ? CommandType.Unknown : message.ElementAt(1).CommandType;
                    switch ( type)
                    {
                        case CommandType.Confirmation:
                            _outgoingConfirmations[message.Id] = message;
                            OutgoingConfirmationSend(message);
                            break;

                        case CommandType.Master:
                            _outgoingSent[message.Id] = message;
                            OutgoingMasterMessageHandler(message);
                            break;

                        case CommandType.Slave:
                            _outgoingSent[message.Id] = message;
                            OutgoingSlaveMessageHandler(message);
                            break;

                        default:
                            _outgoingErrors[message.Id] = (MessageErrors.UndeterminedType, message);
                            OutgoingErrorrousMessageHandler(message);
                            break;
                    }
                }
                else
                {
                    _outgoingErrors[message.Id] = (MessageErrors.NotSent, message);
                    OutgoingErrorrousMessageHandler(message);
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
                OnCommandReceived(this, new CommandEventArgs(command));

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
            if (message is null || message.Count() < 2)
                return IncomingType.Undetermined;

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

        private bool CompletitionTimeoutExceeded(IMessageModel message)
        {
            if (message is null)
                return false;

            if (Timestamp.Difference(message.Timestamp) > _completitionTimeout)
                return true;
            return false;
        }

        private bool ConfirmationTimeoutExceeded(IMessageModel message)
        {
            if (message is null)
                return false;

            if (Timestamp.Difference(message.Timestamp) > _confirmationTimeout)
                return true;
            return false;
        }

        public Dictionary<string, IMessageModel> FindConfirmationTimeouts()
        {
            var output = _outgoingSent.Where(message => ConfirmationTimeoutExceeded(message.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return output;
        }

        public Dictionary<string, IMessageModel> FindCompletitionTimeouts()
        {
            var output = _incomingPartial.Where(message => CompletitionTimeoutExceeded(message.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return output;
        }

        public void ProcessConfirmationTimeouts(Dictionary<string, IMessageModel> timeouts)
        {
            if (timeouts is null || timeouts.Count == 0)
                return;

            foreach (var kvp in timeouts)
            {
                MessageErrors decodingErrors = ErrorCheckWithoutTimeouts(kvp.Value);
                decodingErrors = decodingErrors | MessageErrors.ConfirmationTimeout;

                if (SwitchBuffers(kvp.Value, decodingErrors, _outgoingSent, _outgoingErrors))
                {
                    OutgoingErrorrousMessageHandler(kvp.Value);
                }
            }
        }

        public bool ProcessConfirmationTimeout(IMessageModel message)
        {
            if (message is null || _outgoingSent.ContainsKey(message.Id) == false)
                return false;

            MessageErrors decodingErrors = ErrorCheckWithoutTimeouts(message);
            decodingErrors = decodingErrors | MessageErrors.ConfirmationTimeout;

            if (SwitchBuffers(message, decodingErrors, _outgoingSent, _outgoingErrors))
            {
                OutgoingErrorrousMessageHandler(message);
                return true;
            }
            return false;
        }

        public void ProcessCompletitionTimeouts(Dictionary<string, IMessageModel> timeouts)
        {
            if (timeouts is null || timeouts.Count == 0)
                return;

            foreach (var kvp in timeouts)
            {
                MessageErrors decodingErrors = ErrorCheckWithoutTimeouts(kvp.Value);
                decodingErrors = decodingErrors | MessageErrors.CompletitionTimeout;

                if (SwitchBuffers(kvp.Value, decodingErrors, _incomingPartial, _incomingErrors))
                {
                    IncomingErrorrousMessageHandler(kvp.Value);
                }
            }
        }

        public bool ProcessCompletitionTimeout(IMessageModel message)
        {
            if (message is null || _incomingPartial.ContainsKey(message.Id) == false)
                return false;

            MessageErrors decodingErrors = ErrorCheckWithoutTimeouts(message);
            decodingErrors = decodingErrors | MessageErrors.CompletitionTimeout;

            if (SwitchBuffers(message, decodingErrors, _incomingPartial, _incomingErrors))
            {
                IncomingErrorrousMessageHandler(message);
                return true;
            }
            return false;
        }

        private static MessageErrors ErrorCheckWithoutTimeouts(IMessageModel message)
        {
            if (message is null)
                return MessageErrors.IsNull;

            MessageErrors decodingErrors = CheckIfDecodedCorrectly(message);
            IncomingType messageType = MessageType(message);

            if (IsPatternCorrect(message) == false) decodingErrors = decodingErrors | MessageErrors.BadMessagePattern;
            if (messageType == IncomingType.Undetermined) decodingErrors = decodingErrors | MessageErrors.UndeterminedType;
            if (IsCompleted(message) == false) decodingErrors = decodingErrors | MessageErrors.Incomplete;

            return decodingErrors;
        }

        #endregion Error, state and type checking

        #region Internal helpers

        private IMessageModel CreateConfirmationOf(IMessageModel message)
        {
            if (message is null || message.Count() < 2 || message.ElementAt(1).CommandType == CommandType.Confirmation)
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

        private bool InConfirmationWindow(IMessageModel message)
        {
            if (Timestamp.Difference(message.Timestamp) <= _confirmationTimeout)
            {
                return true;
            }
            return false;
        }

        private bool InCompletitionWindow(IMessageModel message)
        {
            if (Timestamp.Difference(message.Timestamp) <= _completitionTimeout)
            {
                return true;
            }
            return false;
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

        protected virtual void OnIncomingConfirmationReceived(object sender, MessageEventArgs e)
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

        protected virtual void OnOutgoingMasterSent(object sender, MessageEventArgs e)
        {
            OutgoingMasterSent?.Invoke(sender, e);
        }

        protected virtual void OnOutgoingSlaveSent(object sender, MessageEventArgs e)
        {
            OutgoingSlaveSent?.Invoke(sender, e);
        }

        protected virtual void OnOutgoingConfirmationSent(object sender, MessageEventArgs e)
        {
            OutgoingConfirmationSent?.Invoke(sender, e);
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

        internal enum IncomingType
        {
            Master,
            Slave,
            Confirmation,
            Undetermined
        }

        #endregion Internal Enums

        public enum Buffer
        {
            None,
            IncomingPartial,
            IncomingCompleteConfirms,
            IncomingCompleteSlaves,
            IncomingCompleteMasters,
            IncomingError,
            OutgoingSent,
            OutgoingConfirmed,
            OutgoingConfirmations,
            OutgoingErrors
        }
    }
}