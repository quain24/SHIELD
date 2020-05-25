using System;

namespace Shield.Messaging.Commands
{
    public class Timeout
    {
        public Timeout(int timeout)
        {
            Value = timeout > 0 ? timeout : throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout value has to be positive");
        }

        private int Value { get; }

        public bool IsExceeded(Timestamp timestamp) => TimestampFactory.Timestamp.Difference(timestamp) > Value;
    }
}