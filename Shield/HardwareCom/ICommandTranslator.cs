using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface ICommandTranslator
    {
        string FromCommand(ICommandModel givenCommand);

        ICommandModel FromString(string rawData);
    }
}