using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Linq;

namespace Shield.HardwareCom.MessageProcessing
{
    public class Completeness : ICompleteness
    {
        public bool IsComplete(IMessageModel message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            if (message.IsCompleted)
                return true;

            if (message.Count() >= 4 && IsSecondCommandAType(message.Commands[1].CommandType) && message.Last().CommandType == CommandType.EndMessage)
                return message.IsCompleted = true;
            return false;
        }

        private bool IsSecondCommandAType(CommandType type) =>
            type == CommandType.Master || type == CommandType.Slave || type == CommandType.Confirmation;
    }
}