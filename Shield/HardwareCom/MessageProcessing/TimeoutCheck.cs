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
        private long _timeout;
        private const int NoTimeout = 0;

        public int NoTimeoutValue => NoTimeout;

        public TimeoutCheck(long timeout = NoTimeout)
            => _timeout = SetTimeout(timeout);

        public long Timeout
        {
            get => _timeout;
            set => _timeout = SetTimeout(value);
        }

        private long SetTimeout(long timeout)
        {
            return (timeout <= NoTimeout || timeout % 1 != 0) ? NoTimeout : timeout;
        }

        public virtual bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null)
        {
            if (message is null || Timeout <= NoTimeoutValue)
                return false;

            long difference = inCompareTo is null ?
                Timestamp.Difference(message.Timestamp) :
                Timestamp.Difference(message.Timestamp, inCompareTo.Timestamp);
            if (difference > Timeout)
                Debug.WriteLine($@"Timeout - difference: {difference}");
            return difference > Timeout;
        }

        public virtual List<T> GetTimeoutsFromCollection<T>(Dictionary<string, T> source) where T : IMessageModel
            => source is null ? null : GetTimeoutsFromCollection(source.Values);

        public virtual List<T> GetTimeoutsFromCollection<T>(ConcurrentDictionary<string, T> source) where T : IMessageModel
            => source is null ? null : GetTimeoutsFromCollection(source.Values);

        public virtual List<T> GetTimeoutsFromCollection<T>(IEnumerable<T> source) where T : IMessageModel
        {
            try
            {
                return TryGetTimeoutsFrom(source);
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine($@"Something happened when trying to get Timeouts list: {e.Message}");
                return null;
            }
        }

        private List<T> TryGetTimeoutsFrom<T>(IEnumerable<T> source) where T : IMessageModel
        {
            if (source is null)
                return null;

            return source.Where(message => IsExceeded(message))
                         .Select(message => message)
                         .ToList();
        }
    }
}