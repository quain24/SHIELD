using Shield.HardwareCom.Models;
using System.Runtime.Remoting.Messaging;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IMessageAnalyzer
    {
        IMessageModel CheckAndSetFlagsIn(IMessageModel message);
        IMessageModel ClearFlagsIn(IMessageModel message);
    }
}