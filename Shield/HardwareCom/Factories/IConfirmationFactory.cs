using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    public interface IConfirmationFactory
    {
        IMessageHWComModel GenetateConfirmationOf(IMessageHWComModel message);
    }
}