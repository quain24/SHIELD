using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.Commands
{
    public class KnownCommandTypes : IEnumerable<string>
    {
        private readonly HashSet<string> _types;

        public KnownCommandTypes(IEnumerable<string> types)
        {
            _types = Sanitize(types?.ToHashSet()
                ?? throw new ArgumentNullException(nameof(types), $"Cannot create {nameof(KnownCommandTypes)} instance from NULL"));
        }

        public int Count { get => _types.Count; }

        #region IEnumerable<string> implementation

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_types).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_types).GetEnumerator();

        #endregion IEnumerable<string> implementation

        public bool Contains(string type) => _types.Contains(type);

        private HashSet<string> Sanitize(HashSet<string> source)
        {
            HashSet<string> sanitizedTypes = new HashSet<string>();
            foreach (var entry in source)
            {
                var trimmedEntry = entry.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedEntry))
                    sanitizedTypes.Add(trimmedEntry);
            }

            return sanitizedTypes;
        }
    }
}