using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ITimeout
    {
        int TimeoutValue { get; }

        bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null);
    }
}