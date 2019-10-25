using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class ComCommander
    {
        private Func<ICommandModel> _commandFactory;
        private Func<IMessageModel> _messageFactory;
        private IMessanger _messanger;

        #region Sent messages collections

        private ConcurrentQueue<IMessageModel> _outgoingQueue = new ConcurrentQueue<IMessageModel>();
        private Dictionary<string, IMessageModel> _outgoingSent = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _outgoingConfirmed = new Dictionary<string, IMessageModel>();
        private Dictionary<string, (MessageErrors errorType, IMessageModel Message)> _outgoingErrors = new Dictionary<string, (MessageErrors errorType, IMessageModel Message)>();

        #endregion Sent messages collections

        #region Received messages collections

        private Dictionary<string, IMessageModel> _incomingPartial = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingComplete = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingCompleteConfirms = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingCompleteSlaves = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingCompleteMasters = new Dictionary<string, IMessageModel>();
        private Dictionary<string, (MessageErrors errorType, IMessageModel Message)> _incomingErrors = new Dictionary<string, (MessageErrors errorType, IMessageModel Message)>();

        #endregion Received messages collections

        private bool _hasCommunicationManager = false;

        public bool DeviceIsOpen { get { return _hasCommunicationManager ? _messanger.IsOpen : false; } }

        public ComCommander(Func<ICommandModel> commandFactory, Func<IMessageModel> messageFactory)
        {
            // AutoFac auto factories
            _commandFactory = commandFactory;
            _messageFactory = messageFactory;
        }

        public void AssignMessanger(IMessanger messanger)
        {
            if (messanger is null)
                return;
            if (_hasCommunicationManager)
            {
                _messanger.CommandReceived -= OnCommandReceived;
            }

            _messanger = messanger;
            _messanger.CommandReceived += OnCommandReceived;
            _hasCommunicationManager = true;
        }

        public void IncomingConductor(IMessageModel message)
        {
            // Check for decoding correctness
            MessageErrors decodingError = CheckIfDecodedCorrectly(message);
            if (decodingError == MessageErrors.None)
            {
                // Check if message is complete
                if (IsCompleted(message))
                {
                    // Message is completed, decoded properly - now off to corresponding buffers and handlers
                    SwitchBuffers(message, _incomingPartial, _incomingComplete);
                    if (IsPatternCorrect(message))
                    {
                        switch (MessageType(message))
                        {
                            case IncomingType.Confirmation:
                                SwitchBuffers(message, _incomingComplete, _incomingCompleteConfirms);
                                IncomingConfirmationMessageHandler(message);
                                break;

                            case IncomingType.Master:
                                SwitchBuffers(message, _incomingComplete, _incomingCompleteMasters);
                                IncomingMasterMessageHandler(message);
                                break;

                            case IncomingType.Slave:
                                SwitchBuffers(message, _incomingComplete, _incomingCompleteSlaves);
                                IncomingSlaveMessageHandler(message);
                                break;

                            default:
                                SwitchBuffers(message, MessageErrors.UndeterminedType, _incomingComplete, _incomingErrors);
                                IncomingErrorrousMessageHandler(message);
                                break;
                        }
                    }
                    // Something wrong in message pattern
                    else
                    {
                        if (MessageType(message) == IncomingType.Undetermined)
                        {
                            SwitchBuffers(message, MessageErrors.UndeterminedType, _incomingComplete, _incomingErrors);
                            IncomingErrorrousMessageHandler(message);
                        }
                        else
                        {
                            SwitchBuffers(message, MessageErrors.BadMessagePattern, _incomingComplete, _incomingErrors);
                            IncomingErrorrousMessageHandler(message);
                        }
                    }
                }
                // Message incomplete
                else
                {
                    // Nothing, wait for another command
                    return;
                }
            }
            // Message is incorrect - throw it into IncomingErrors
            else
            {
                SwitchBuffers(message, decodingError, _incomingPartial, _incomingErrors);
                IncomingErrorrousMessageHandler(message);
            }
        }

        // Handles Incoming Confirmations and moves confirmed messages to correct buffers
        private void IncomingConfirmationMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            // If there is nothing to confirm, then exit
            if (!_outgoingSent.ContainsKey(message.Id))
            {
                SwitchBuffers(message, MessageErrors.ConfirmedNonexistent, _incomingCompleteConfirms, _incomingErrors);
                return;
            }

            // If confirmed exists, then time to check if receiving device actually got message properly
            _incomingCompleteConfirms[message.Id] = message;

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
                if (SwitchBuffers(_outgoingSent[message.Id], _outgoingSent, _outgoingConfirmed))
                {
                    OutgoingConfirmedMessagehandler(_outgoingConfirmed[message.Id]);
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
            MessageErrors error;

            // Error checking Conditions and insert into outgoing error buffer
            if (receivedPartials > 0 && receivedErrors > 0 && receivedUnknowns > 0)
                error = MessageErrors.GotErrorAndUnknownAndPartialCommands;

            if (receivedUnknowns > 0 && receivedErrors > 0)
                error = MessageErrors.GotErrorAndUnknownCommands;

            if (receivedUnknowns > 0 && receivedPartials > 0)
                error = MessageErrors.GotPartialAndUnknownCommands;

            if (receivedPartials > 0 && receivedErrors > 0)
                error = MessageErrors.GotErrorAndPartialCommands;

            if (receivedUnknowns > 0)
                error = MessageErrors.GotUnknownCommands;

            if (receivedPartials > 0)
                error = MessageErrors.GotPartialsCommands;

            if (receivedErrors > 0)
                error = MessageErrors.GotErrorCommands;
            else
                error = MessageErrors.Unknown;

            SwitchBuffers(tmpErrMessage, error, _outgoingSent, _outgoingErrors);
            OutgoingErrorrousMessageHandler(tmpErrMessage);

            return;
        }

        // If incoming message could not be decoded correctly (unknown commands, errors), then its handled here
        public void IncomingErrorrousMessageHandler(IMessageModel message)
        {
            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            Console.WriteLine($@"ERROR TYPE: {Enum.GetName(typeof(MessageErrors), _incomingErrors[message.Id].errorType)}");
            foreach(var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if(c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public void OutgoingErrorrousMessageHandler(IMessageModel message)
        {
            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach(var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if(c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public void OutgoingConfirmedMessagehandler(IMessageModel message)
        {
            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach(var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if(c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public void IncomingMasterMessageHandler(IMessageModel message)
        {
            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach(var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if(c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public void IncomingSlaveMessageHandler(IMessageModel message)
        {
            Console.WriteLine($@"From: {System.Reflection.MethodBase.GetCurrentMethod().Name} | ID: {message.Id} | Command count: {message.CommandCount}");
            foreach(var c in message)
            {
                string line = $@"----  {c.CommandTypeString}";
                if(c.CommandType == CommandType.Data)
                    line += $@" | Data: {c.Data}";
                Console.WriteLine(line);
            }
        }

        public bool AddToSendingQueue(IMessageModel message)
        {
            if (message is null)
                return false;

            message.Timestamp = Timestamp.TimestampNow;
            message.IsOutgoing = true;
            _outgoingQueue.Enqueue(message);
            return true;
        }

        public async Task<bool> SendNextQueuedMessageAsync()
        {
            IMessageModel message;
            bool wasSent = false;
            if (_outgoingQueue.TryDequeue(out message))
            {
                wasSent = await _messanger.SendAsync(message).ConfigureAwait(false);
                message.Timestamp = Timestamp.TimestampNow;
                if (wasSent)
                {
                    _outgoingSent[message.Id] = message;
                }
                else
                {
                    _outgoingErrors[message.Id] = (MessageErrors.NotSent, message);
                }
            }
            return wasSent;
        }

        public void Receiver(ICommandModel receivedCommand)
        {
            Buffer commandDepositedIn = ChooseBufferFor(receivedCommand);
            string messageId = receivedCommand.Id;

            if (commandDepositedIn == Buffer.IncomingPartial)
            {
                IncomingConductor(_incomingPartial[messageId]);
            }                           
        }

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
                if (_incomingPartial[command.Id].IsTransmissionCompleted)
                {
                    IMessageModel newMessage = _messageFactory();
                    newMessage.Timestamp = Timestamp.TimestampNow;
                    newMessage.AssaignID(command.Id);
                    newMessage.Add(command);
                    _incomingErrors[command.Id] = (MessageErrors.WasAlreadyCompleted, newMessage);
                    return Buffer.IncomingError;
                }
                _incomingPartial[command.Id].Add(command);
            }
            else
            {
                IMessageModel newMessage = _messageFactory();
                newMessage.Timestamp = Timestamp.TimestampNow;
                newMessage.AssaignID(command.Id);
                newMessage.Add(command);
                newMessage.IsIncoming = true;
                _incomingPartial[newMessage.Id] = newMessage;
            }
            return Buffer.IncomingPartial;
        }

        private static MessageErrors CheckIfDecodedCorrectly(IMessageModel message)
        {
            if (message is null)
                return MessageErrors.isNull;

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

            foreach (ICommandModel c in badOrUnknown)
            {
                if (c.CommandType == CommandType.Error)
                    numberOfErrors++;
                else if (c.CommandType == CommandType.Unknown)
                    numberOfUnknowns++;
                else if (c.CommandType == CommandType.Partial)
                    numberOfPartials++;
            }

            if (numberOfErrors > 0 && numberOfUnknowns > 0 && numberOfPartials > 0)
                return MessageErrors.GotErrorAndUnknownAndPartialCommands;
            if (numberOfErrors > 0 && numberOfPartials > 0)
                return MessageErrors.GotErrorAndPartialCommands;
            if (numberOfErrors > 0 && numberOfUnknowns > 0)
                return MessageErrors.GotErrorAndUnknownCommands;
            if (numberOfPartials > 0 && numberOfUnknowns > 0)
                return MessageErrors.GotPartialAndUnknownCommands;
            if (numberOfErrors > 0)
                return MessageErrors.GotErrorCommands;
            if (numberOfPartials > 0)
                return MessageErrors.GotPartialsCommands;
            else
                return MessageErrors.GotUnknownCommands;
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
                return false;

            // Only one message type?
            if (message.
                Count(c =>
                    c.CommandType == CommandType.Master ||
                    c.CommandType == CommandType.Slave ||
                    c.CommandType == CommandType.Confirmation)
                != 1)
                return false;

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

        public virtual void OnCommandReceived(object sender, ICommandModel command)
        {
            Receiver(command);
        }

        #region Message transfer between buffers

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
            if (!Enum.IsDefined(typeof(MessageErrors), error))
                return false;

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

        #endregion Message transfer between buffers
    }

    public enum MessageErrors
    {
        None,
        GotPartialsCommands,
        GotUnknownCommands,
        GotErrorCommands,
        GotErrorAndUnknownCommands,
        GotPartialAndUnknownCommands,
        GotErrorAndPartialCommands,
        GotErrorAndUnknownAndPartialCommands,
        UndeterminedType,
        BadMessagePattern,
        NotSent,
        NotConfirmed,
        ConfirmedNonexistent,
        Empty,
        WasAlreadyCompleted,
        Unknown,
        isNull
    }

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
}