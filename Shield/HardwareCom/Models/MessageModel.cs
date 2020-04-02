using Shield.HardwareCom.Enums;
using System.Collections;
using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public class MessageModel : IMessageModel
    {
        private string _messageId = string.Empty;

        #region IEnumerable implementation

        public IEnumerator<ICommandModel> GetEnumerator()
        {
            for (int i = 0; i < Commands.Count; i++)
                yield return Commands[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable implementation

        public long Timestamp { get; set; }
        public string Id { get { return _messageId; } set { AssaignID(value); } }
        public MessageType Type { get; set; } = MessageType.Unknown;
        public Direction Direction { get; set; } = Direction.Unknown;
        public Errors Errors { get; set; } = Errors.None;
        public List<ICommandModel> Commands { get; } = new List<ICommandModel>();
        public int CommandCount => Commands.Count;

        public bool IsConfirmed { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public bool IsCorrect => Errors == Errors.None;
        public bool IsTransfered { get; set; } = false;

        #region Indexer

        public int Count { get => Commands.Count; }

        public ICommandModel this[int index]
        {
            get => Commands[index];
            set => Commands[index] = value;
        }

        #endregion Indexer

        public string AssaignID(string id)
        {
            _messageId = id.ToUpperInvariant();

            if (Commands != null)
                Commands.ForEach(command => command.Id = _messageId);
            return _messageId;
        }

        public bool Add(ICommandModel command)
        {
            if (command is null)
                return false;
            command.Id = _messageId;
            Commands.Add(command);
            return true;
        }

        public bool Remove(ICommandModel command)
        {
            return Commands.Remove(command);
        }

        public bool Replace(ICommandModel target, ICommandModel replacement)
        {
            var targetIndex = Commands.IndexOf(target);
            if (targetIndex == -1)
                return false;
            Commands[targetIndex] = replacement;
            return true;
        }
    }
}