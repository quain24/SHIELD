using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    public interface ICommandModelFactory
    {
        ICommandModel Create(CommandType type = CommandType.Empty);
    }
}