using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shield.Enums;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public static class MessageTypeDetector
    {
        public static IncomingMessageType Detect(IMessageModel message)
        {
            if(message is null)
                throw new ArgumentNullException(nameof(message), "MessageTypeDetector: Detect - Passed NULL instead of a Message object");

            if (message.Count() < 2)
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
    }
}
