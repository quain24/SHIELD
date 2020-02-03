using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ITypeDetector
    {
        MessageType DetectTypeOf(IMessageModel message);
    }
}