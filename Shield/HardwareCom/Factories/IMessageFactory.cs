using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    public interface IMessageFactory
    {
        IMessageHWComModel CreateNew(
            Direction direction = Direction.Unknown,
            MessageType type = MessageType.Unknown,
            string id = "",
            long timestampOverride = 0);
    }
}