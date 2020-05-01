using Shield.HardwareCom.MessageProcessing;
using System;

namespace Shield.HardwareCom.Factories
{
    public class NormalTimeoutFactory : ITimeoutConcreteFactory
    {
        private readonly Func<int, NormalTimeout> _timeotFactoryAutoFac;

        public NormalTimeoutFactory(Func<int, NormalTimeout> timeoutFactoryAutoFac)
        {
            _timeotFactoryAutoFac = timeoutFactoryAutoFac ?? throw new ArgumentNullException(nameof(timeoutFactoryAutoFac));
        }

        public ITimeout CreateTimeoutWith(int milliseconds) => _timeotFactoryAutoFac(milliseconds);
    }
}