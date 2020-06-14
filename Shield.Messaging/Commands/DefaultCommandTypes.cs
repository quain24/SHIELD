using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.Commands
{
    public abstract class DefaultCommandTypes : IEnumerable<string>
    {
        private readonly HashSet<string> _commands = new HashSet<string>()
        {
            "Data",
            "Master",
            "Slave",
            "Confirmation",
            "EndMessage",
            "ReceivedAsCorrect",
            "ReceivedAsError",
            "ReceivedAsUnknown",
            "ReceivedAsPartial",
            "ConfirmationTimeoutOccurred",
            "CompletitionTimeoutOccured",
            "Confirm",
            "Cancel",
            "RetryLast"
        };

        protected DefaultCommandTypes(IEnumerable<string> additionalCommandTypes)
        {
            if (additionalCommandTypes is null)
                return;
            _commands.UnionWith(Sanitize(additionalCommandTypes.ToHashSet()));
        }

        public int Count { get => _commands.Count; }

        #region IEnumerable<string> implementation

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_commands).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_commands).GetEnumerator();

        #endregion IEnumerable<string> implementation

        public bool Contains(string type) => _commands.Contains(type, StringComparer.OrdinalIgnoreCase);

        protected virtual HashSet<string> Sanitize(HashSet<string> source)
        {
            var sanitizedTypes = new HashSet<string>();
            foreach (var entry in source)
            {
                if (!string.IsNullOrWhiteSpace(entry))
                    sanitizedTypes.Add(entry.Trim());
            }

            return sanitizedTypes;
        }
    }
}