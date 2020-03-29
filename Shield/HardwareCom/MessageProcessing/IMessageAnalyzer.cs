using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IMessageAnalyzer
    {
        IMessageModel CheckAndSetFlagsIn(IMessageModel message);

        IMessageModel ClearFlagsIn(IMessageModel message);
    }
}