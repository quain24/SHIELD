using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ITimeoutCheck
    {
        int Timeout { get; }

        bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null);
    }
}