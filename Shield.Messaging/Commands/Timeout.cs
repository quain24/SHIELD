using System;
using Shield.Timestamps;

namespace Shield.Messaging.Commands
{
    public class Timeout
    {
        public Timeout(int timeoutInSeconds)
        {
            ValueInTicks = timeoutInSeconds > 0
                ? TimeSpan.FromSeconds(timeoutInSeconds).Ticks
                : throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds), "Timeout value has to be positive");
            ValueInSeconds = timeoutInSeconds;
        }

        public int InMilliseconds => (int)TimeSpan.FromTicks(ValueInTicks).TotalSeconds * 1000;

        private long ValueInTicks { get; }
        private int ValueInSeconds { get; }

        public bool IsExceeded(Timestamp timestamp) => TimestampFactory.Timestamp.Difference(timestamp) > ValueInTicks;

        public override string ToString() => ValueInTicks.ToString();

        public static implicit operator int(Timeout timeout) => timeout.ValueInSeconds;
    }
}