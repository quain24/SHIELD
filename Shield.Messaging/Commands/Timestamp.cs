using System;
using System.Collections.Generic;

namespace Shield.Messaging.Commands
{
    public sealed class Timestamp : IEquatable<Timestamp>, IComparable<Timestamp>
    {
        private readonly long _value;

        public Timestamp(long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Timestamp cannot be negative.");
            _value = value;
        }

        #region IEquatable<Timestamp> implementation

        public override bool Equals(object obj)
        {
            return Equals(obj as Timestamp);
        }

        public bool Equals(Timestamp other)
        {
            return other != null &&
                   _value == other._value;
        }

        public override int GetHashCode()
        {
            return -1939223833 + _value.GetHashCode();
        }

        public static bool operator ==(Timestamp left, Timestamp right)
        {
            return EqualityComparer<Timestamp>.Default.Equals(left, right);
        }

        public static bool operator !=(Timestamp left, Timestamp right)
        {
            return !(left == right);
        }

        #endregion IEquatable<Timestamp> implementation

        #region IComparable<Timestamp> implementation

        public int CompareTo(Timestamp other) => _value.CompareTo(other._value);

        public static bool operator <(Timestamp left, Timestamp right) => left.CompareTo(right) == -1;

        public static bool operator >(Timestamp left, Timestamp right) => left.CompareTo(right) == 1;

        public static bool operator <=(Timestamp left, Timestamp right) => left < right || left == right;

        public static bool operator >=(Timestamp left, Timestamp right) => left > right || left == right;

        #endregion IComparable<Timestamp> implementation

        public long Difference(Timestamp other) => _value - other._value;

        public long ToLong() => _value;

        public override string ToString() => _value.ToString();

        // TODO where to put IsExceededd method - here, in timestamp or in Timeout object. Or as separate static?
    }
}