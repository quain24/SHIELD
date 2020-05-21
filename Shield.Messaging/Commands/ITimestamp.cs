using System;

namespace Shield.Messaging.Commands
{
    public interface ITimestamp : IEquatable<ITimestamp>, IComparable<ITimestamp>
    {
        long ToLong();
    }
}