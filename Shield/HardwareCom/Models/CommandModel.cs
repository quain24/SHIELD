using Shield.Enums;
using System;

namespace Shield.HardwareCom.Models
{
    /// <summary>
    /// Basic type for communication with a machine - single command encapsulates a single command type or a single data 'row'
    /// </summary>
    public class CommandModel : ICommandModel
    {
        private string _data = string.Empty;
        private string _id = string.Empty;
        private long _timestamp = 0;
        private CommandType _command;

        public long TimeStamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public CommandType CommandType
        {
            get { return _command; }
            set { _command = value; }
        }

        public string CommandTypeString
        {
            get { return Enum.GetName(typeof(CommandType), CommandType); }
        }

        public bool Equals(ICommandModel other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id.Equals(other.Id) && Data.Equals(other.Data) && CommandType.Equals(other.CommandType);
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