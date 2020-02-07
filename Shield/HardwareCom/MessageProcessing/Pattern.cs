using Shield.Enums;
using Shield.HardwareCom.Models;
using System.Linq;

namespace Shield.HardwareCom.MessageProcessing
{
    public class Pattern : IPattern
    {
        public bool IsCorrect(IMessageModel message)
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
    }
}