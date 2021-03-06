﻿using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Models;
using System;

namespace Shield.HardwareCom.Factories
{
    public class ConfirmationFactory : IConfirmationFactory
    {
        private ICommandModelFactory _commandFactory;
        private IMessageFactory _messageFactory;
        private readonly string _hostId;

        public ConfirmationFactory(ICommandModelFactory commandFactory, IMessageFactory messageFactory, string hostId)
        {

            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            _hostId = hostId;
        }

        public IMessageModel GenerateConfirmationOf(IMessageModel message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message), "ConfirmationFactory - GenerateConfirmationOf: Cannot create confirmation of NULL");

            IMessageModel confirmation = _messageFactory.CreateNew(Direction.Outgoing,
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

            if (message.Errors.HasFlag(Errors.CompletitionTimeout))
                confirmation.Add(_commandFactory.Create(CommandType.CompletitionTimeoutOccured));

            if (message.Errors.HasFlag(Errors.ConfirmationTimeout))
                confirmation.Add(_commandFactory.Create(CommandType.ConfirmationTimeoutOccurred));

            confirmation.Add(_commandFactory.Create(CommandType.EndMessage));

            // Assigns id also to all commands inside
            confirmation.AssaignID(message.Id);
            confirmation.AssighHostID(_hostId);

            return confirmation;
        }
    }
}