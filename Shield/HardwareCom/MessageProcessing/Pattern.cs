using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Linq;

namespace Shield.HardwareCom.MessageProcessing
{
    public class Pattern : IMessageAnalyzer
    {
        private IMessageModel _message;

        public IMessageModel CheckAndSetFlagsIn(IMessageModel message)
        {
            _message = ClearFlagsIn(message);

            if (IsOfMinimalLength() &&
                IsBeginningAndEndPositionCorrect() &&
                HasOneBeginningAndEnd() &&
                IsMessageTypeInCorrectPlace() &&
                HasOnlyOneTypeCommand())
                return _message;
            else
            {
                _message.Errors |= Errors.BadMessagePattern;
                return _message;
            }
        }

        public IMessageModel ClearFlagsIn(IMessageModel message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            if (message.Errors.HasFlag(Errors.BadMessagePattern))
                message.Errors &= ~Errors.BadMessagePattern;
            return message;
        }

        private bool IsOfMinimalLength() =>
            _message.Count() < 3 ? false : true;

        private bool IsBeginningAndEndPositionCorrect()
        {
            if (_message.First().CommandType != CommandType.HandShake ||
                _message.Last().CommandType != CommandType.EndMessage)
                return false;
            return true;
        }

        private bool HasOneBeginningAndEnd() =>
            _message.Count(c => c.CommandType == CommandType.HandShake && c.CommandType == CommandType.EndMessage) != 2;

        private bool IsMessageTypeInCorrectPlace()
        {
            CommandType messageType = _message.ElementAt(1).CommandType;
            return (messageType == CommandType.Master ||
                    messageType == CommandType.Slave ||
                    messageType == CommandType.Confirmation);
        }

        private bool HasOnlyOneTypeCommand()
        {
            return _message.Count(c =>
                        c.CommandType == CommandType.Master ||
                        c.CommandType == CommandType.Slave ||
                        c.CommandType == CommandType.Confirmation) == 1;
        }
    }
}