using Shield.HardwareCom.Models;
using Shield.HardwareCom.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class MessageFactory : IMessageFactory
    {
        private readonly Func<IMessageModel> _messageFactory;

        public MessageFactory(Func<IMessageModel> messageFactory)
        {
            _messageFactory = messageFactory;
        }

        public IMessageModel CreateNew(
            Enums.Direction direction = Enums.Direction.Unknown,
            Enums.MessageType type = Enums.MessageType.Unknown,
            string idOverride = "",
            string hostIdOverride = "",
            long timestampOverride = 0)
        {
            IMessageModel output = _messageFactory();
            output.AssaignID(string.IsNullOrWhiteSpace(idOverride) ? string.Empty : idOverride);
            output.AssighHostID(string.IsNullOrWhiteSpace(hostIdOverride)? string.Empty : hostIdOverride);
            output.Type = type;
            output.Direction = direction;
            output.Timestamp = timestampOverride < 0 ? Timestamp.TimestampNow : timestampOverride;
            return output;
        }
    }
}