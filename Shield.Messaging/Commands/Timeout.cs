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
            ValueInSeconds = timeoutInSeconds;
        }

        public int InMilliseconds => (int)TimeSpan.FromTicks(Value).TotalSeconds * 1000;

        private long Value { get; }
        private int ValueInSeconds { get; }

        public bool IsExceeded(Timestamp timestamp) => TimestampFactory.Timestamp.Difference(timestamp) > Value;

        public override string ToString() => Value.ToString();

        public static implicit operator int(Timeout timeout) => timeout.ValueInSeconds;
    }
}