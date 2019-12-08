using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IDecoding
    {
        Errors Check(IMessageHWComModel message);
    }
}