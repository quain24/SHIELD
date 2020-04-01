using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.MessageProcessing
{
    public class NormalTimeout : ITimeout
    {
        private readonly int _timeout;

        public NormalTimeout(int milliseconds) =>
            _timeout = milliseconds > 0
            ? milliseconds
            : throw new ArgumentOutOfRangeException(nameof(milliseconds), "timeout has to have more than 0 milliseconds");

        public int TimeoutValue => _timeout;

        /// <summary>
        /// Checks if <paramref name="message"/> timeout was exceeded.
        /// If <paramref name="inCompareTo"/> message was supplied, then it will check <paramref name="message"/> Timeout against <paramref name="inCompareTo"/>
        /// Timeout and positive difference will be returned
        /// </summary>
        /// <param name="message">Message in which timeout is checked</param>
        /// <param name="inCompareTo">Optional message that main message timeout will be checked against</param>
        /// <returns>True if timeout was larger than <see cref="TimeoutValue"/> value</returns>
        public virtual bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            long difference = inCompareTo is null ?
                Timestamp.Difference(message.Timestamp) :
                Timestamp.Difference(message.Timestamp, inCompareTo.Timestamp);
            return difference > _timeout;
        }
    }
}