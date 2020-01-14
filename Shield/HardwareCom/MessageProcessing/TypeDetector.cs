using Shield.Enums;
using Shield.HardwareCom.Models;
using System.Linq;

namespace Shield.HardwareCom.MessageProcessing
{
    public class TypeDetector : ITypeDetector
    {
        public MessageType DetectTypeOf(IMessageHWComModel message)
        {
            if (message is null || message.Count() < 2)
                return MessageType.Unknown;

            if (message.Type != MessageType.Unknown)
                return message.Type;

            CommandType type = message.ElementAt(1).CommandType;

            switch (type)
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