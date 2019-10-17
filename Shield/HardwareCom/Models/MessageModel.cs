using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains list of commands that will be sent or were received
/// Single message is preffered unit of communication between devices
/// </summary>

namespace Shield.HardwareCom.Models
{
    public class MessageModel : IMessageModel
    {
        private string _messageId = string.Empty;
        private List<ICommandModel> _commands = new List<ICommandModel>();

        public bool IsBeingSent { get; set; } = false;
        public bool IsBeingReceived { get; set; } = false;
        public bool IsIncoming { get; set; } = false;
        public bool IsOutgoing { get; set; } = false;
        public bool IsTransmissionCompleted { get; set; } = false;
        public int CommandCount { get { return _commands.Count(); } }
        public string Id { get { return _messageId; } set { AssaignID(value); } }

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
            if (!string.IsNullOrEmpty(_messageId))
                command.Id = _messageId;
            _commands.Add(command);
            return true;
        }

        public bool Remove(ICommandModel command)
        {
            int countBefore = CommandCount;
            foreach (var c in _commands)
            {
                if (c.Equals(command))
                {
                    _commands.Remove(c);
                }
            }

            return countBefore != CommandCount ? true : false;
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