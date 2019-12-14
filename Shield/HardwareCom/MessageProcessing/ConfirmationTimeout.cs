using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;

namespace Shield.HardwareCom.MessageProcessing
{
    public class ConfirmationTimeout : IConfirmationTimeout
    {
        private long _confirmationTimeout;

        public ConfirmationTimeout (long timeout = 0)
        {
            _confirmationTimeout = timeout;
        }

        public long Timeout
        {
            get => _confirmationTimeout;
            set => _confirmationTimeout = value;
        }

        public bool IsExceeded(IMessageHWComModel message)
        {
            if (message is null || Timeout == 0)
                return false;

            if (message.Errors.HasFlag(Errors.ConfirmationTimeout))
                return true;

            if (Timestamp.Difference(message.Timestamp) > _confirmationTimeout)
                return true;
            return false;
        }
    }
}