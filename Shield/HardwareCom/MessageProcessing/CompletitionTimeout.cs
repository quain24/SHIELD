using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;

namespace Shield.HardwareCom.MessageProcessing
{
    public class CompletitionTimeout : TimeoutCheck
    {
        public CompletitionTimeout(long timeout)
            : base(timeout) { }

        public override bool IsExceeded(IMessageHWComModel message)
        {
            if (message is null || Timeout <= 0)
                return false;

            if (message.Errors.HasFlag(Errors.CompletitionTimeout))
                return true;

            if (Timestamp.Difference(message.Timestamp) > _timeout)
                return true;
            return false;
        }
    }
}