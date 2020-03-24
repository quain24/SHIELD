using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.CommandProcessing
{
    public interface ICommandTranslator
    {
        string FromCommand(ICommandModel givenCommand);

        ICommandModel FromString(string rawData);
    }
}