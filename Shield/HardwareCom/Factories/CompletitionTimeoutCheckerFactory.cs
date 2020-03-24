using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public class CompletitionTimeoutCheckerFactory : ICompletitionTimeoutCheckerFactory
    {
        public ICompletitionTimeoutChecker CreateCompletitionTimoutCheckerUsing(ICommandIngester ingesterToWorkWith, ITimeoutCheck completitionTimeoutChecker) =>
            new CompletitionTimeoutChecker(ingesterToWorkWith, completitionTimeoutChecker);
    }
}