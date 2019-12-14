using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;

namespace Shield.HardwareCom.MessageProcessing
{
    public class CompletitionTimeout : ICompletitionTimeout
    {
        private long _completitionTimeout;

        public CompletitionTimeout(long timeout = 0)
        {
            _completitionTimeout = timeout;
        }

        public long Timeout
        {
            get => _completitionTimeout;
            set => _completitionTimeout = value;
        }

        public bool IsExceeded(IMessageHWComModel message)
        {
            if (message is null || Timeout == 0)
                return false;

            if (message.Errors.HasFlag(Errors.CompletitionTimeout))
                return true;

            if (Timestamp.Difference(message.Timestamp) > _completitionTimeout)
                return true;
            return false;
        }
    }
}