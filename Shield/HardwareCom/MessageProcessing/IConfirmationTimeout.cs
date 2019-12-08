using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IConfirmationTimeout
    {
        long Timeout { get; set; }

        bool IsExceeded(IMessageHWComModel message);
    }
}