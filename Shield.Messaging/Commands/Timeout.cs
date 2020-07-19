using Shield.Timestamps;
using System;

namespace Shield.Messaging.Commands
{
    public sealed class Timeout
    {
        public Timeout(int inMilliseconds)
        {
            ValueInTicks = inMilliseconds > 0
                ? TimeSpan.FromMilliseconds(inMilliseconds).Ticks
                : throw new ArgumentOutOfRangeException(nameof(inMilliseconds), "Timeout value has to be positive");
            ValueInSeconds = inMilliseconds * 1000;
        }

        public int InMilliseconds => (int)TimeSpan.FromTicks(ValueInTicks).TotalSeconds * 1000;

        private long ValueInTicks { get; }
        private int ValueInSeconds { get; }

        public bool IsExceeded(Timestamp timestamp) => TimestampFactory.Timestamp.Difference(timestamp) > ValueInTicks;

        public override string ToString() => ValueInTicks.ToString();

        public static implicit operator int(Timeout timeout) => timeout.ValueInSeconds;
    }
}