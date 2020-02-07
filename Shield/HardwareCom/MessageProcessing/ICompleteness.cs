using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ICompleteness
    {
        bool IsComplete(IMessageModel message);
    }
}