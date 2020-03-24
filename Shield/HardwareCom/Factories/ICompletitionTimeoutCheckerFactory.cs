using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public interface ICompletitionTimeoutCheckerFactory
    {
        ICompletitionTimeoutChecker GetCheckerUsing(ICommandIngester ingesterToWorkWith, ITimeoutCheck completitionTimeoutChecker);
    }
}