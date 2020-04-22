using Shield.HardwareCom.Enums;
using System;

namespace Shield.HardwareCom.Models
{
    /// <summary>
    /// Basic type for communication with a machine - single command encapsulates a single command type or a single data 'row'
    /// </summary>
    public class CommandModel : ICommandModel
    {
        private string _id = string.Empty;

        public long TimeStamp { get; set; } = 0;

        public string Id
        {
            get { return _id; }
            set { _id = value?.ToUpperInvariant() ?? string.Empty; }
        }

        public string Data { get; set; } = string.Empty;

        public CommandType CommandType { get; set; }

        public string CommandTypeString => Enum.GetName(typeof(CommandType), CommandType);

        public bool Equals(ICommandModel other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase) &&
                   Data.Equals(other.Data, StringComparison.OrdinalIgnoreCase) &&
                   CommandType.Equals(other.CommandType, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            int hash = 9;
            int hashId = string.IsNullOrEmpty(Id) ? 0 : Id.GetHashCode();
            int hashData = string.IsNullOrEmpty(Data) ? 0 : Data.GetHashCode();
            int hashCommandType = CommandType.GetHashCode();

            hash = unchecked(3 * (hash + hashData + hashId + hashCommandType));
            return hash;
        }
    }
}