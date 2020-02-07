using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Linq;

namespace Shield.HardwareCom.MessageProcessing
{
    public class Completeness : ICompleteness
    {
        private object _lock = new object();

        public bool IsComplete(IMessageModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "Tried to check a NULL instead of a message");

            if (message.IsCompleted == true || (message.Any() &&  message.Last().CommandType == CommandType.EndMessage))
            {
                message.IsCompleted = true;
                return true;
            }

            return false;
        }
    }
}