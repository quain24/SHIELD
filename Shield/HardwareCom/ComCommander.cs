using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Shield.Data;
using Shield.Helpers;

namespace Shield.HardwareCom
{
    public class ComCommander
    {
        private Func<ICommandModel> _commandFactory;
        private Func<IMessageModel> _messageFactory;
        private IAppSettings _appSettings;

        private IMessanger _messanger;
        private Dictionary<string, IMessageModel> _incomingBuffer = new Dictionary<string, IMessageModel>();
        private Dictionary<string, IMessageModel> _outgoingBuffer = new Dictionary<string, IMessageModel>();
        private IMessageModel _currentMessage;

        private int _idLength;
        private bool _hasCommunicationManager = false;

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
                _messanger.Dispose();
            }

            _messanger = messanger;
            _messanger.CommandReceived += OnCommandReceived;
            _hasCommunicationManager = true;
        }

        public void SendMessage(IMessageModel message)
        {
            if (message is null)
                return;

            message.IsOutgoing = true;
            _outgoingBuffer.Add(message.Id, message);
        }

        public void Conductor()
        {
        }

        public void Responder()
        {
        }

        public void Sender()
        {
        }

        public void Receiver(ICommandModel receivedCommand)
        {
            //  check type - new or responding?
            //  if responding, then check outgoing messages if there was one with corresponding id
            //  NO - Decide if user should be notified or wait in buffer and try again later?
            //  YES - Check if there are errors in response
            //      NO - Message is correct, return it to user / object handling correct messages
            //      YES - Check error type, try to correct by resending if data missing or partial
            //            or notify user / object that one of commands is not recognized

            AddIncomingToBuffer(receivedCommand);
        }

        private void AddIncomingToBuffer(ICommandModel command)
        {
            if (command is null)
                return;

            if (_incomingBuffer.ContainsKey(command.Id))
            {
                _incomingBuffer[command.Id].Add(command);
            }
            else
            {
                IMessageModel newMessage = _messageFactory();
                newMessage.AssaignID(IdGenerator.GetId(_idLength));
                newMessage.Add(command);
                newMessage.IsIncoming = true;
                _incomingBuffer.Add(newMessage.Id, newMessage);
            }
        }

        private bool CheckIfCompleted(IMessageModel message, CommandTrasnferDirection direction)
        {
            if (message is null)
                return false;

            Dictionary<string, IMessageModel> bufferToCheck;

            switch (direction)
            {
                case CommandTrasnferDirection.Incoming:
                    bufferToCheck = _incomingBuffer;
                    break;

                case CommandTrasnferDirection.Outgoing:
                    bufferToCheck = _outgoingBuffer;
                    break;

                case CommandTrasnferDirection.Unknown:
                    return false;

                default:
                    Debug.WriteLine("ERROR: ComCommander - CheckIfCompleted - Wrong enum value passed - returning false");
                    return false;
            }

            if (bufferToCheck.ContainsKey(message.Id))
            {
                if (bufferToCheck[message.Id].Last().CommandType == CommandType.Completed)
                {
                    return true;
                }
            }
            return false;
        }

        private MessageCorectness AnalyseIfCorrect(IMessageModel message)
        {
            if (message is null)
                return MessageCorectness.isNull;

            List<ICommandModel> badOrCompleted = message
                .Where(m =>
                    m.CommandType == CommandType.Unknown ||
                    m.CommandType == CommandType.Error)
                .ToList();

            if (!badOrCompleted.Any())
                return MessageCorectness.Empty;

            int numberOfUnknowns = 0;
            int numberOfErrors = 0;

            foreach (ICommandModel c in badOrCompleted)
            {
                if (c.CommandType == CommandType.Error)
                    numberOfErrors++;
                else if (c.CommandType == CommandType.Unknown)
                    numberOfUnknowns++;
            }

            if (numberOfErrors > 0 && numberOfUnknowns > 0)
                return MessageCorectness.GotErrorsAndUnknowns;
            if (numberOfErrors > 0)
                return MessageCorectness.GotErrors;
            if (numberOfUnknowns > 0)
                return MessageCorectness.GotUnknowns;

            return MessageCorectness.Good;
        }

        public virtual void OnCommandReceived(object sender, ICommandModel command)
        {
            Receiver(command);
        }
    }

    internal enum CommandTrasnferDirection
    {
        Incoming,
        Outgoing,
        Unknown
    }

    internal enum MessageCorectness
    {
        Good,
        GotUnknowns,
        GotErrors,
        GotErrorsAndUnknowns,
        Empty,
        isNull
    }
}