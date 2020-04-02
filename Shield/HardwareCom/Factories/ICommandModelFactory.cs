using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    public interface ICommandModelFactory
    {
        ICommandModel Create(CommandType type = CommandType.Empty, string idOverride = "", long timestampOverride = 0);
    }
}