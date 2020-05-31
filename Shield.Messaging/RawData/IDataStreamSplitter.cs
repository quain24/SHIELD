using System.Collections.Generic;

namespace Shield.Messaging.RawData
{
    public interface IDataStreamSplitter
    {
        IEnumerable<RawCommand> Split(string data);
    }
}