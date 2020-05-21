using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.RawData
{
    public class RawCommandCollection : IEnumerable<string>
    {
        private readonly List<string> _rawCommands;

        public RawCommandCollection(IEnumerable<string> rawCommands)
        {
            _rawCommands = rawCommands?.ToList() ?? new List<string>();
        }

        public int Count => _rawCommands.Count;

        public RawCommandCollection GetCommandsOf(int length) =>
            new RawCommandCollection(_rawCommands.Where(r => r.Length == length));

        public IEnumerator<string> GetEnumerator()
        {
            return _rawCommands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}