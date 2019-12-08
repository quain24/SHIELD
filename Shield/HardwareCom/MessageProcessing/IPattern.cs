using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IPattern
    {
        bool IsCorrect(IMessageHWComModel message);
    }
}