using Shield.HardwareCom.MessageProcessing;
using System;

namespace Shield.HardwareCom.Factories
{
    public class ConfirmationTimeoutCheckerFactory : IConfirmationTimeoutCheckerFactory
    {
        private readonly ITimeoutCheckFactory _timeoutCheckFactory;

        public ConfirmationTimeoutCheckerFactory(ITimeoutCheckFactory timeoutCheckFactory)
        {
            _timeoutCheckFactory = timeoutCheckFactory ?? throw new ArgumentNullException(nameof(timeoutCheckFactory));
        }

        public IConfirmationTimeoutChecker GetCheckerUsing(int timeout) =>
            new ConfirmationTimeoutChecker(_timeoutCheckFactory.GetTimeoutCheckWithTimeoutSetTo(timeout));
    }
}