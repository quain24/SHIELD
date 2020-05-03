using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    public interface IMessageFactory
    {
        IMessageModel CreateNew(
            Direction direction = Direction.Unknown,
            MessageType type = MessageType.Unknown,
            string id = "",
            string hostId = "",
            long timestampOverride = 0);
    }
}