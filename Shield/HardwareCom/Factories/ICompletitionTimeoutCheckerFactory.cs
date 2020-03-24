using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public interface ICompletitionTimeoutCheckerFactory
    {
        ICompletitionTimeoutChecker CreateCompletitionTimoutCheckerUsing(ICommandIngesterAlt ingesterToWorkWith, ITimeoutCheck completitionTimeoutChecker);
    }
}