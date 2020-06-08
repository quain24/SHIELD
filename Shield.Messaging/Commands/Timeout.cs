using System;

namespace Shield.Messaging.Commands
{
    public class Timeout
    {
        public Timeout(int timeoutInSeconds)
        {
            Value = timeoutInSeconds > 0
                ? TimeSpan.FromSeconds(timeoutInSeconds).Ticks
                : throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds), "Timeout value has to be positive");
        }

        private long Value { get; }

        public bool IsExceeded(Timestamp timestamp) => TimestampFactory.Timestamp.Difference(timestamp) > Value;

        public override string ToString() => Value.ToString();
    }
}