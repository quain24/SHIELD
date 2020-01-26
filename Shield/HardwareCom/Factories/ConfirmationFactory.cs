using Shield.Enums;
using Shield.HardwareCom.Models;
using System;

namespace Shield.HardwareCom.Factories
{
    public class ConfirmationFactory : IConfirmationFactory
    {
        private ICommandModelFactory _commandFactory;
        private IMessageFactory _messageFactory;

        public ConfirmationFactory(ICommandModelFactory commandFactory, IMessageFactory messageFactory)
        {
            _commandFactory = commandFactory;
            _messageFactory = messageFactory;
        }

        public IMessageHWComModel GenetateConfirmationOf(IMessageHWComModel message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message), "ConfirmationFactory - GenerateConfirmationOf: Cannot create confirmation of NULL");

            IMessageHWComModel confirmation = _messageFactory.CreateNew(Direction.Outgoing,
                                                                        MessageType.Confirmation,
                                                                        message.Id);

            confirmation.Add(_commandFactory.Create(CommandType.HandShake));
            confirmation.Add(_commandFactory.Create(CommandType.Confirmation));

            foreach (var c in message)
            {
                ICommandModel responseCommand = _commandFactory.Create(timestampOverride: confirmation.Timestamp);
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

            // Assigns id also to all commands inside
            confirmation.AssaignID(message.Id);

            return confirmation;
        }
    }
}