using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.RawData
{
    public class RawCommandCollection : IEnumerable<RawCommand>
    {
        private readonly List<RawCommand> _rawCommands;

        public RawCommandCollection(IEnumerable<RawCommand> rawCommands)
        {
            _rawCommands = rawCommands?.ToList() ?? new List<RawCommand>();
        }

        public int Count => _rawCommands.Count;

        #region IEnumerable<string> implementation

        public IEnumerator<RawCommand> GetEnumerator()
        {
            return _rawCommands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable<string> implementation

        public RawCommandCollection GetCommandsOf(int length) =>
            new RawCommandCollection(_rawCommands.Where(r => r.Length == length));
    }
}