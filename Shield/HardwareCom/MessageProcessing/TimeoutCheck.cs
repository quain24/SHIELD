using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public abstract class TimeoutCheck : ITimeoutCheck
    {
        internal long _timeout;

        public TimeoutCheck(long timeout = 0)
        {
            _timeout = SetTimeout(timeout);
            System.Console.WriteLine("base call");
        }

        public long Timeout
        {
            get => _timeout;
            set => _timeout = SetTimeout(value);
        }

        private long SetTimeout(long timeout)
        {
            return (timeout <= 0 || timeout % 1 != 0) ? 0 : timeout;
        }

        public abstract bool IsExceeded(IMessageHWComModel message);
    }
}