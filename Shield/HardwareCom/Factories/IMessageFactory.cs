using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    public interface IMessageFactory
    {
        IMessageHWComModel CreateNew(
            string id = "",            
            Direction direction = Direction.Unknown,
            MessageType type = MessageType.Unknown,
            long timestampOverride = 0);
    }
}