using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ITimeoutCheck
    {
        long Timeout { get; set; }

        bool IsExceeded(IMessageHWComModel message);
    }
}