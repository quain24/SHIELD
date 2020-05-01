using Shield.HardwareCom.Enums;
using Shield.HardwareCom.MessageProcessing;
using System;
using System.Collections.Generic;

namespace Shield.HardwareCom.Factories
{
    public class TimeoutFactory : ITimeoutFactory
    {
        private readonly IReadOnlyDictionary<TimeoutType, ITimeoutConcreteFactory> _timeoutFactories;

        public TimeoutFactory(IReadOnlyDictionary<TimeoutType, ITimeoutConcreteFactory> timeoutFactories)
        {
            _timeoutFactories = timeoutFactories ?? throw new ArgumentNullException(nameof(timeoutFactories));
        }

        public ITimeout CreateTimeoutWith(int milliseconds)
        {
            return milliseconds <= 0
                ? _timeoutFactories[TimeoutType.NullTimeout].CreateTimeoutWith(milliseconds)
                : _timeoutFactories[TimeoutType.NormalTimeout].CreateTimeoutWith(milliseconds);
        }
    }
}