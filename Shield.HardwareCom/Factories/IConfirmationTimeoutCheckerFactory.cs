using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public interface IConfirmationTimeoutCheckerFactory
    {
        IConfirmationTimeoutChecker GetCheckerUsing(int timeout);
    }
}