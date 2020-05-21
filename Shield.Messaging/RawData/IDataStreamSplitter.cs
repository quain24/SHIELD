using System.Collections.Generic;

namespace Shield.Messaging.RawData
{
    public interface IDataStreamSplitter
    {
        IEnumerable<string> Split(string data);
    }
}