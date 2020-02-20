using Shield.Enums;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Linq;

namespace Shield.HardwareCom
{
    public class Decoding : IMessageAnalyzer
    {
        private IMessageModel _message;

        public IMessageModel CheckAndSetFlagsIn(IMessageModel message)
        {
            _message = ClearFlagsIn(message);

            if (HasCommandsOfType(CommandType.Unknown))
                SetErrorFlag(Errors.GotUnknownCommands);
            if (HasCommandsOfType(CommandType.Error))
                SetErrorFlag(Errors.GotErrorCommands);
            if (HasCommandsOfType(CommandType.Partial))
                SetErrorFlag(Errors.GotPartialCommands);

            return _message;
        }

        public IMessageModel ClearFlagsIn(IMessageModel message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            ClearErrorFlag(Errors.GotUnknownCommands, message);
            ClearErrorFlag(Errors.GotErrorCommands, message);
            ClearErrorFlag(Errors.GotPartialCommands, message);

            return message;
        }

        private void ClearErrorFlag(Errors error, IMessageModel message)
        {
            if (message.Errors.HasFlag(error))
                message.Errors &= ~error;
        }

        private bool HasCommandsOfType(CommandType type) =>
            _message.Any(c => c.CommandType == type);

        private void SetErrorFlag(Errors error) =>
            _message.Errors |= error;
    }
}