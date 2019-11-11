using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains list of commands that will be sent or were received
/// Single message is preferred unit of communication between devices
/// </summary>

namespace Shield.HardwareCom.Models
{
    public class MessageModel : IMessageModel
    {
        private string _messageId = string.Empty;
        private List<ICommandModel> _commands = new List<ICommandModel>();

        public long Timestamp { get; set; } = 0;
        public int CommandCount { get { return _commands.Count(); } }
        public string Id { get { return _messageId; } set { AssaignID(value); } }

        public List<ICommandModel> Commands { get { return _commands; } }

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
            if(targetIndex == -1)
                return false;
            _commands[targetIndex] = replacement;
            return true;
        }

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
    }
}