using Shield.Enums;
using System.Collections;
using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public class MessageHWComModel : IMessageHWComModel
    {
        private string _messageId = string.Empty;
        private Errors _errors = Errors.None;
        private List<ICommandModel> _commands = new List<ICommandModel>();

        #region IEnumerable implementation

        public IEnumerator<ICommandModel> GetEnumerator()
        {
            for (int i = 0; i < _commands.Count; i++)
                yield return _commands[i];
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
        public List<ICommandModel> Commands { get { return _commands; } }

        public bool Confirmed { get; set; } = false;
        public bool Completed { get; set; } = false;
        public bool Correct { get; set; } = false;
        public bool Transfered { get; set; } = false;

        #region Indexer

        public int Count { get => _commands.Count; }

        public ICommandModel this[int index]
        {
            get => _commands[index];
            set => _commands[index] = value;
        }

        #endregion Indexer

        public string AssaignID(string id)
        {
            _messageId = id.ToUpperInvariant();

            foreach (var c in _commands)
            {
                c.Id = _messageId;
            }

            return _messageId;
        }

        public bool Add(ICommandModel command)
        {
            if (command is null)
                return false;
            command.Id = _messageId;
            _commands.Add(command);
            return true;
        }

        public bool Remove(ICommandModel command)
        {
            return _commands.Remove(command);
        }

        public bool Replace(ICommandModel target, ICommandModel replacement)
        {
            var targetIndex = _commands.IndexOf(target);
            if (targetIndex == -1)
                return false;
            _commands[targetIndex] = replacement;
            return true;
        }
    }
}