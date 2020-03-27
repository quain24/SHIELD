using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class MessageFactory : IMessageFactory
    {
        private Func<IMessageModel> _messageFactory;
        private readonly IIdGenerator _idGenerator;

        public MessageFactory(Func<IMessageModel> messageFactory, IIdGenerator idGenerator)
        {
            _messageFactory = messageFactory;
            _idGenerator = idGenerator;
        }

        public IMessageModel CreateNew(
            Enums.Direction direction = Enums.Direction.Unknown,
            Enums.MessageType type = Enums.MessageType.Unknown,
            string idOverride = "",
            long timestampOverride = 0)
        {
            IMessageModel output = _messageFactory();
            output.AssaignID((string.IsNullOrWhiteSpace(idOverride) ? _idGenerator.GetNewID() : idOverride));
            output.Type = type;
            output.Direction = direction;
            output.Timestamp = (timestampOverride <= 0 ? Timestamp.TimestampNow : timestampOverride);
            return output;
        }
    }
}