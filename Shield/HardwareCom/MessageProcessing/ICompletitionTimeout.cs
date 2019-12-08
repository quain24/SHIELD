using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ICompletitionTimeout
    {
        long Timeout { get; set; }

        bool IsExceeded(IMessageHWComModel message);
    }
}