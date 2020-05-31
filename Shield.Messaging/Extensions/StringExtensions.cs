using Shield.Messaging.RawData;
using System.Collections.Generic;

namespace Shield.Messaging.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<RawCommand> ToRawCommands(this IEnumerable<string> data)
        {
            var collection = new List<RawCommand>();
            foreach (var entry in data)
                collection.Add(new RawCommand(entry));
            return collection;
        }
    }
}