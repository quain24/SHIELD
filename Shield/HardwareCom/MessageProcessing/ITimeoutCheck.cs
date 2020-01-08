using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ITimeoutCheck
    {
        long Timeout { get; set; }
        int NoTimeoutValue { get; }

        bool IsExceeded(IMessageHWComModel message, IMessageHWComModel inCompareTo = null);
    }
}