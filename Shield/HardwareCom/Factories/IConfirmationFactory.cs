using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    public interface IConfirmationFactory
    {
        IMessageModel GenetateConfirmationOf(IMessageModel message);
    }
}