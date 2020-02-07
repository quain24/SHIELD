using Shield.HardwareCom.Models;
using Shield.Helpers;
using System.Collections.Generic;

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
            if (difference > Timeout) System.Console.WriteLine($@"Timeout - difference: {difference}");
            return difference > Timeout ? true : false;
        }

        public virtual List<T> GetTimeoutsFromCollection<T>(Dictionary<string, T> source) where T : IMessageModel
        {
            return source is null ? null : GetTimeoutsFromCollection(source.Values);
        }

        public virtual List<T> GetTimeoutsFromCollection<T>(IEnumerable<T> source) where T : IMessageModel
        {
            if (source is null)
                return null;

            List<T> output = new List<T>();

            foreach (T message in source)
            {
                if (IsExceeded(message))
                {
                    output.Add(message);
                }
            }

            return output;
        }
    }
}