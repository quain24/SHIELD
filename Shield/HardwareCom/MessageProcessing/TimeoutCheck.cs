using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Shield.HardwareCom.MessageProcessing
{
    public class TimeoutCheck : ITimeoutCheck
    {
        private readonly int _timeout;
        
        public TimeoutCheck(int timeoutInMilliseconds) =>
            _timeout = timeoutInMilliseconds > 0
            ? timeoutInMilliseconds
            : throw new ArgumentOutOfRangeException(nameof(timeoutInMilliseconds), $@"{nameof(timeoutInMilliseconds)} should be a positive whole number");

        public int Timeout => _timeout;

        /// <summary>
        /// Checks if <paramref name="message"/> timeout was exceeded.
        /// If <paramref name="inCompareTo"/> message was supplied, then it will check <paramref name="message"/> Timeout against <paramref name="inCompareTo"/>
        /// Timeout and positive difference will be returned
        /// </summary>
        /// <param name="message">Message in which timeout is checked</param>
        /// <param name="inCompareTo">Optional message that main message timeout will be checked against</param>
        /// <returns>True if timeout was larger than <see cref="Timeout"/> value</returns>
        public virtual bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            long difference = inCompareTo is null ?
                Timestamp.Difference(message.Timestamp) :
                Timestamp.Difference(message.Timestamp, inCompareTo.Timestamp);
            if (difference > Timeout)
                Debug.WriteLine($@"Timeout - difference: {difference}");
            return difference > Timeout;
        }
    }
}