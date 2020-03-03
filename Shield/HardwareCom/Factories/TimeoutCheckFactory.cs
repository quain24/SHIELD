using Shield.HardwareCom.MessageProcessing;
using System;

namespace Shield.HardwareCom.Factories
{
    public class TimeoutCheckFactory
    {
        private readonly Func<int, ITimeoutCheck> _timeoutCheckAutofactory;
        private readonly ITimeoutCheck _nullTimeoutCheck;

        public TimeoutCheckFactory(Func<int, ITimeoutCheck> timeoutCheckAutofactory, ITimeoutCheck nullTimeoutCheck)
        {
            _timeoutCheckAutofactory = timeoutCheckAutofactory;
            _nullTimeoutCheck = nullTimeoutCheck;
        }

        public ITimeoutCheck GetTimeoutCheckWithTimeoutSetTo(int milliseconds) =>
            milliseconds <= 0 ? _nullTimeoutCheck : _timeoutCheckAutofactory(milliseconds);
    }
}