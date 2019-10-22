using Shield.Data;
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
        private IAppSettings _appSettings;
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
        private Dictionary<string, IMessageModel> _incomingCompleteConfirmed = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _incomingCompleteResponses = new Dictionary<string, IMessageModel>();
        private Dictionary<string, (MessageErrors errorType, IMessageModel Message)> _incomingErrors = new Dictionary<string, (MessageErrors errorType, IMessageModel Message)>();

        #endregion Received messages collections

        private int _idLength;
        private bool _hasCommunicationManager = false;

        private object _incomingBufferLock = new object();

        public bool DeviceIsOpen { get { return _hasCommunicationManager ? _messanger.IsOpen : false; } }

        public ComCommander(Func<ICommandModel> commandFactory, Func<IMessageModel> messageFactory, IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _idLength = _appSettings.GetSettingsFor<Data.Models.IApplicationSettingsModel>().IdSize;

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

        public void Conductor()
        {
        }

        // Handles Incoming Confirmations and moves confirmed messages to correct buffers
        private void IncomingConfirmationMessageHandler(IMessageModel message)
        {
            if (message is null)
                return;

            // If there is nothing to confirm, then exit
            if (!_outgoingSent.ContainsKey(message.Id))
            {
                _incomingErrors[message.Id] = (MessageErrors.ConfirmedNonexistent, message);
                _incomingComplete.Remove(message.Id);
                return;
            }

            // If confirmed existing, then time to check if receiving device did actually got message properly
            _incomingCompleteResponses[message.Id] = message;
            _incomingComplete.Remove(message.Id);

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
                _outgoingConfirmed[message.Id] = message;
                _outgoingSent.Remove(message.Id);
                return;
            }

            // Something went bad - sent message was received with errors.
            // Pushing message that was confirmed to error buffer (at the end) and removing from sent buffer

            IMessageModel tmpErrMessage = _outgoingSent[message.Id];
            _outgoingSent.Remove(tmpErrMessage.Id);

            // Error checking Conditions and insert into outgoing error buffer
            if (receivedPartials > 0 && receivedErrors > 0 && receivedUnknowns > 0)
            {
                _outgoingErrors[message.Id] = (MessageErrors.GotErrorsAndUnknownsAndPartials, tmpErrMessage);
            }

            if (receivedUnknowns > 0 && receivedErrors > 0)
            {
                _outgoingErrors[message.Id] = (MessageErrors.GotErrorsAndUnknowns, tmpErrMessage);
            }

            if (receivedUnknowns > 0 && receivedPartials > 0)
            {
                _outgoingErrors[message.Id] = (MessageErrors.GotPartialsAndUnknowns, tmpErrMessage);
            }

            if (receivedPartials > 0 && receivedErrors > 0)
            {
                _outgoingErrors[message.Id] = (MessageErrors.GotErrorsAndPartials, tmpErrMessage);
            }

            if (receivedUnknowns > 0)
            {
                _outgoingErrors[message.Id] = (MessageErrors.GotUnknowns, tmpErrMessage);
            }

            if (receivedPartials > 0)
            {
                _outgoingErrors[message.Id] = (MessageErrors.GotPartials, tmpErrMessage);
            }

            if (receivedErrors > 0)
            {
                _outgoingErrors[message.Id] = (MessageErrors.GotErrors, tmpErrMessage);
            }

            _outgoingSent.Remove(tmpErrMessage.Id);
            return;
        }

        // If incoming message could not be decoded correctly (unknown commands, errors), then its handled here
        private void IncomingErrorrousMessageHandler(IMessageModel message)
        {
        }

        private void IncomingMasterMessageHandler(IMessageModel message)
        {
        }

        private void IncomingSlaveMessageHandler(IMessageModel message)
        {
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
            Buffer commandDepositedIn = AddIncomingToBuffer(receivedCommand);
            string messageId = receivedCommand.Id;

            // Add received command to existing or new message in buffer
            if (commandDepositedIn == Buffer.IncomingPartial)
            {
                // Message is completed
                if (CheckIfCompleted(_incomingPartial[messageId]))
                {
                    _incomingComplete[messageId] = _incomingPartial[messageId];
                    _incomingPartial.Remove(messageId);

                    var corectness = CheckIfDecodedCorrectly(_incomingComplete[messageId]);
                    var messageType = MessageType(_incomingComplete[messageId]);

                    // There are no decoding errors in completed message
                    if (corectness == MessageErrors.None)
                    {
                        switch (messageType)
                        {
                            case IncomingType.Confirmation:
                                IncomingConfirmationMessageHandler(_incomingComplete[messageId]);
                                break;

                            case IncomingType.Master:
                                IncomingMasterMessageHandler(_incomingComplete[messageId]);
                                break;

                            case IncomingType.Slave:
                                IncomingSlaveMessageHandler(_incomingComplete[messageId]);
                                break;

                            default:
                                // If undetermined message type
                                IncomingErrorrousMessageHandler(_incomingComplete[messageId]);
                                break;
                        }
                    }
                }
            }
        }

        private Buffer AddIncomingToBuffer(ICommandModel command)
        {
            if (command is null)
                return Buffer.None;

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

        private static bool CheckIfCompleted(IMessageModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "Cannot pass null message!");

            if (message.IsTransmissionCompleted)
                return true;

            if (message.Last().CommandType == CommandType.Completed)
            {
                message.IsTransmissionCompleted = true;
                return true;
            }

            return false;
        }

        private static IncomingType MessageType(IMessageModel message)
        {
            List<ICommandModel> statusCommands = message
                    .Where(c =>
                        c.CommandType == CommandType.Confirmation ||
                        c.CommandType == CommandType.Master ||
                        c.CommandType == CommandType.Slave)
                    .ToList();
            if (statusCommands.Count == 1)
            {
                switch (statusCommands.First().CommandType)
                {
                    case CommandType.Confirmation:
                        return IncomingType.Confirmation;

                    case CommandType.Master:
                        return IncomingType.Master;

                    case CommandType.Slave:
                        return IncomingType.Slave;

                    default:
                        throw new InvalidOperationException("Outside of bound for CommandType Enum - was it changed, were there memory errors?");
                }
            }

            return IncomingType.Undetermined;
        }

        /// <summary>
        /// Checks if received message is properly decoded
        /// </summary>
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
                return MessageErrors.GotErrorsAndUnknownsAndPartials;
            if (numberOfErrors > 0 && numberOfPartials > 0)
                return MessageErrors.GotErrorsAndPartials;
            if (numberOfErrors > 0 && numberOfUnknowns > 0)
                return MessageErrors.GotErrorsAndUnknowns;
            if (numberOfPartials > 0 && numberOfUnknowns > 0)
                return MessageErrors.GotPartialsAndUnknowns;
            if (numberOfErrors > 0)
                return MessageErrors.GotErrors;
            if (numberOfPartials > 0)
                return MessageErrors.GotPartials;
            else
                return MessageErrors.GotUnknowns;
        }

        public virtual void OnCommandReceived(object sender, ICommandModel command)
        {
            Receiver(command);
        }
    }

    internal enum MessageErrors
    {
        None,
        GotPartials,
        GotUnknowns,
        GotErrors,
        GotErrorsAndUnknowns,
        GotPartialsAndUnknowns,
        GotErrorsAndPartials,
        GotErrorsAndUnknownsAndPartials,
        CannotTellType,
        NotSent,
        NotConfirmed,
        ConfirmedNonexistent,
        Empty,
        WasAlreadyCompleted,
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