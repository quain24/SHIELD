using System;

namespace Shield.Messaging.Commands
{
    public sealed class Timestamp : ITimestamp
    {
        private readonly long _value;

        public Timestamp(long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Timestamp cannot be negative.");
            _value = value;
        }

        #region IEquatable<ITimestamp> implementation

        public override bool Equals(object obj) => Equals(obj as ITimestamp);

        public bool Equals(ITimestamp other)
        {
            return other is ITimestamp timestamp &&
                   _value == timestamp.ToLong();
        }

        public override int GetHashCode() => -1939223833 + _value.GetHashCode();

        #endregion IEquatable<ITimestamp> implementation

        #region IComparable<ITimestamp> implementation

        public int CompareTo(ITimestamp other)
        {
            if (_value < other.ToLong()) return 1;
            else if (_value > other.ToLong()) return -1;
            else return 0;
        }

        #endregion IComparable<ITimestamp> implementation

        public long ToLong() => _value;

        public override string ToString() => _value.ToString();
        // TODO where to put IsExceededd method - here, in timestamp or in Timeout object. Or as separate static?
    }
}