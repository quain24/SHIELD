using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class MessageFactory : IMessageFactory
    {
        private Func<IMessageHWComModel> _messageFactory;
        private int _idLength;

        public MessageFactory(Func<IMessageHWComModel> messageFactory, int idLength)
        {
            _messageFactory = messageFactory;
            _idLength = idLength;
        }

        public IMessageHWComModel CreateNew(
            Enums.Direction direction = Enums.Direction.Unknown,
            Enums.MessageType type = Enums.MessageType.Unknown,            
            string idOverride = "",
            long timestampOverride = 0)
        {
            IMessageHWComModel output = _messageFactory();
            output.AssaignID((string.IsNullOrWhiteSpace(idOverride) ? IdGenerator.GetID(_idLength) : idOverride));
            output.Type = type;
            output.Direction = direction;
            output.Timestamp = (timestampOverride <= 0 ? Timestamp.TimestampNow : timestampOverride);
            return output;
        }
    }
}