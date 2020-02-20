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

            if (message.IsCompleted == true || (message.Any() && message.Last().CommandType == CommandType.EndMessage))
            {
                message.IsCompleted = true;
                return true;
            }

            return false;
        }
    }
}