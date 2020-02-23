using Shield.Enums;
using Shield.HardwareCom.Models;
using System.Linq;

namespace Shield.HardwareCom.MessageProcessing
{
    public class TypeDetector : IMessageAnalyzer
    {
        private IMessageModel _message;

        public IMessageModel CheckAndSetFlagsIn(IMessageModel message)
        {
            _message = ClearFlagsIn(message);

            _message.Type = DetectType();
            if (_message.Type == MessageType.Unknown)
                _message.Errors |= Errors.UndeterminedType;
            return _message;
        }

        public IMessageModel ClearFlagsIn(IMessageModel message)
        {
            _ = message ?? throw new System.ArgumentNullException(nameof(message));

            if (message.Errors.HasFlag(Errors.UndeterminedType))
                message.Errors &= ~Errors.UndeterminedType;
            return message;
        }

        private MessageType DetectType() =>
            CanFindAnyTypeIn()
            ? GetTypeFromCommand(_message.ElementAt(1))
            : MessageType.Unknown;

        private bool CanFindAnyTypeIn() =>
            _message != null && _message.Count() >= 2;

        private MessageType GetTypeFromCommand(ICommandModel command)
        {
            switch (command.CommandType)
            {
                case CommandType.Master:
                return MessageType.Master;

                case CommandType.Slave:
                return MessageType.Slave;

                case CommandType.Confirmation:
                return MessageType.Confirmation;

                default:
                return MessageType.Unknown;
            }
        }
    }
}