using System.Collections.Generic;

namespace Shield.Messaging.Commands
{
    public class KnownCommandTypes : DefaultCommandTypes
    {
        public KnownCommandTypes(IEnumerable<string> types) : base(types)
        {
        }
    }
}