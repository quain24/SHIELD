using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shield.HardwareCom
{
    public class MessageInfoAndErrorChecks : IMessageInfoAndErrorChecks
    {
        private long _confirmationTimeout;
        private long _completitionTimeout;

        public long ConfirmationTimeout
        {
            get { return _confirmationTimeout; }
            set { _confirmationTimeout = value; }
        }

        public long CompletitionTimeout
        {
            get { return _completitionTimeout; }
            set { _completitionTimeout = value; }
        }

        public bool CompletitionTimeoutExceeded(IMessageModel message)
        {
            if (message is null)
                return false;

            if (Timestamp.Difference(message.Timestamp) > _completitionTimeout)
                return true;
            return false;
        }

        public bool ConfirmationTimeoutExceeded(IMessageModel message)
        {
            if (message is null)
                return false;

            if (Timestamp.Difference(message.Timestamp) > _confirmationTimeout)
                return true;
            return false;
        }

        public bool InConfirmationWindow(IMessageModel message)
        {
            if (Timestamp.Difference(message.Timestamp) <= _confirmationTimeout)
            {
                return true;
            }
            return false;
        }

        public bool InCompletitionWindow(IMessageModel message)
        {
            if (Timestamp.Difference(message.Timestamp) <= _completitionTimeout)
            {
                return true;
            }
            return false;
        }

        public MessageErrors DecodingErrorsIn(IMessageModel message)
        {
            if (message is null)
                return MessageErrors.IsNull;

            List<ICommandModel> badOrUnknown = message
                .Where(c =>
                    c.CommandType == CommandType.Unknown ||
                    c.CommandType == CommandType.Error ||
                    c.CommandType == CommandType.Partial)
                .ToList();

            if (badOrUnknown.Any() == false)
                return MessageErrors.None;

            MessageErrors errors = MessageErrors.None;

            foreach (ICommandModel c in badOrUnknown)
            {
                if (c.CommandType == CommandType.Error)
                    errors = errors | MessageErrors.GotErrorCommands;
                else if (c.CommandType == CommandType.Unknown)
                    errors = errors | MessageErrors.GotUnknownCommands;
                else if (c.CommandType == CommandType.Partial)
                    errors = errors | MessageErrors.GotPartialCommands;
            }
            return errors;
        }

        public IncomingMessageType DetectType(IMessageModel message)
        {
            if (message is null || message.Count() < 2)
                return IncomingMessageType.Undetermined;

            CommandType type = message.ElementAt(1).CommandType;

            switch (type)
            {
                case CommandType.Master:
                    return IncomingMessageType.Master;

                case CommandType.Slave:
                    return IncomingMessageType.Slave;

                case CommandType.Confirmation:
                    return IncomingMessageType.Confirmation;

                default:
                    return IncomingMessageType.Undetermined;
            }
        }

        public bool IsPatternCorrect(IMessageModel message)
        {
            // not enough commands in message
            if (message.Count() < 3)
                return false;

            // Correct beginning and end?
            if (message.First().CommandType != CommandType.HandShake ||
                message.Last().CommandType != CommandType.EndMessage)
                return false;

            // Only one begin and one end?
            if (message.Count(c => c.CommandType == CommandType.HandShake ||
                                   c.CommandType == CommandType.EndMessage) != 2)
                return false;

            // Message type in correct place?
            CommandType messageType = message.ElementAt(1).CommandType;
            if (messageType != CommandType.Master &&
                messageType != CommandType.Slave &&
                messageType != CommandType.Confirmation)
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

        public bool IsCompleted(IMessageModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "Cannot pass null message!");

            if (message.Last().CommandType == CommandType.EndMessage)
                return true;

            return false;
        }
    }
}