using Shield.HardwareCom.Models;
using Shield.Helpers;

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
            if(difference > Timeout) System.Console.WriteLine($@"Timeout - difference: {difference}");
            return difference > Timeout ? true : false;
        }
    }
}