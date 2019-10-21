using Shield.Data;
using Shield.Enums;
using Shield.HardwareCom.Models;
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
        private Dictionary<string, IMessageModel> _outgoingSended = new Dictionary<string, IMessageModel>();
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

        public bool AddToSendingQueue(IMessageModel message)
        {
            if (message is null)
                return false;

            message.IsOutgoing = true;
            _outgoingQueue.Enqueue(message);
            return true;
        }

        public void Conductor()
        {
        }

        public void RespondTo(IMessageModel message)
        {
        }

        //public async Task<bool> SendQueuedMessagesAsync()
        //{
        //}

        public async Task<bool> SendNextQueuedMessageAsync()
        {
            IMessageModel message;
            bool wasSent = false;
            if (_outgoingQueue.TryDequeue(out message))
            {
                wasSent = await _messanger.SendAsync(message).ConfigureAwait(false);
                if (wasSent)
                {
                    _outgoingSended[message.Id] = message;
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
            // Add received command to existing or new message in buffer
            if (AddIncomingToBuffer(receivedCommand) == Buffer.IncomingPartial)
            {
                if (CheckIfCompleted(_incomingPartial[receivedCommand.Id]))
                {
                    string id = receivedCommand.Id;

                    _incomingComplete[id] = _incomingPartial[id];
                    _incomingPartial.Remove(id);

                    var status = CheckIfCorrect(_incomingComplete[id]);

                    if (status == MessageErrors.Good)
                    {
                        if (IsResponse(_incomingComplete[id]))
                        {                            
                            if (_outgoingSended.ContainsKey(id))
                            {
                                _outgoingConfirmed[id] = _outgoingSended[id];
                                _outgoingSended.Remove(id);
                                _incomingCompleteResponses[id] = _incomingComplete[id];
                            }
                            else
                            {
                                _incomingErrors[id] = (MessageErrors.ConfirmedNonexistent, _incomingComplete[id]);
                                _incomingComplete.Remove(id);
                            }
                        }
                        _incomingComplete[receivedCommand.Id] = _incomingPartial[receivedCommand.Id];
                        _incomingPartial.Remove(receivedCommand.Id);
                        RespondTo(_incomingComplete[receivedCommand.Id]);
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

        private MessageErrors CheckIfCorrect(IMessageModel message)
        {
            if (message is null)
                return MessageErrors.isNull;

            List<ICommandModel> badOrUnknown = message
                .Where(c =>
                    c.CommandType == CommandType.Unknown ||
                    c.CommandType == CommandType.Error)
                .ToList();

            if (!badOrUnknown.Any())
            {
                List<ICommandModel> statusCommands = message
                    .Where(c =>
                        c.CommandType == CommandType.Confirmation ||
                        c.CommandType == CommandType.Master || 
                        c.CommandType == CommandType.Slave)
                    .ToList();
                if(statusCommands.Count == 1)
                    return MessageErrors.Good;
                else
                    return MessageErrors.CannotTellType;
            }                

            int numberOfUnknowns = 0;
            int numberOfErrors = 0;

            foreach (ICommandModel c in badOrUnknown)
            {
                if (c.CommandType == CommandType.Error)
                    numberOfErrors++;
                else if (c.CommandType == CommandType.Unknown)
                    numberOfUnknowns++;
            }

            if (numberOfErrors > 0 && numberOfUnknowns > 0)
                return MessageErrors.GotErrorsAndUnknowns;
            if (numberOfErrors > 0)
                return MessageErrors.GotErrors;
            else
                return MessageErrors.GotUnknowns;
        }

        private bool IsResponse(IMessageModel message)
        {
            if(message.Any(c => c.CommandType == CommandType.Confirmation))
                return true;
            return false;
        }

        private void IfMessageIsIncorrect()
        {
        }

        public virtual void OnCommandReceived(object sender, ICommandModel command)
        {
            Receiver(command);
        }
    }

    internal enum MessageErrors
    {
        Good,
        GotUnknowns,
        GotErrors,
        GotErrorsAndUnknowns,
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
}