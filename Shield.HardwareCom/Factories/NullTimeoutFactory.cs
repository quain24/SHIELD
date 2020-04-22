using Shield.HardwareCom.MessageProcessing;
using System;

namespace Shield.HardwareCom.Factories
{
    public class NullTimeoutFactory : ITimeoutConcreteFactory
    {
        private readonly NullTimeout _nullTimeout;

        public NullTimeoutFactory(NullTimeout nullTimeout)
        {
            _nullTimeout = nullTimeout ?? throw new ArgumentNullException(nameof(nullTimeout));
        }

        public ITimeout CreateTimeoutWith(int milliseconds = 0) => _nullTimeout;
        
    }
}