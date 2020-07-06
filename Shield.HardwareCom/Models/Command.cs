using Shield.HardwareCom.Enums;
using System;

namespace Shield.HardwareCom.Models
{
    /// <summary>
    /// Basic immutable type for communication with a machine - single command encapsulates a single command type or a single data 'row'
    /// </summary>
    public sealed class Command : ICommand
    {
        public long TimeStamp { get; } = 0;

        public string HostId { get; }

        public string Id { get; }

        public string Data { get; }

        public CommandType CommandType { get; }

        public Command(CommandType type, string id, string hostId, string data, long timestamp)
        {
            CommandType = type;
            Id = id.ToUpperInvariant();
            HostId = hostId.ToUpperInvariant();
            Data = data;
            TimeStamp = timestamp;
        }

        #region IEquatable<Command> implementation

        public override bool Equals(object obj) => Equals(obj as ICommand);

        public bool Equals(ICommand other) =>

            other != null &&
            Id.Equals(other.Id) &&
            Data.Equals(other.Data) &&
            CommandType.Equals(other.CommandType);

        public override int GetHashCode() =>
            Id.GetHashCode() ^ HostId.GetHashCode() ^ Data.GetHashCode() ^ TimeStamp.GetHashCode();

        public static bool operator ==(Command a, Command b) =>
            (a is null && b is null) ||
            (a is object && a.Equals(b));

        public static bool operator !=(Command a, Command b) => !(a == b);

        #endregion IEquatable<Command> implementation
    }
}