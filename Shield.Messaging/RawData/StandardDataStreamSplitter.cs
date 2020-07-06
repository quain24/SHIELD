using Shield.Extensions;
using Shield.Messaging.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.RawData
{
    public class StandardDataStreamSplitter : IDataStreamSplitter
    {
        private readonly char _splitter;
        private string _buffer = string.Empty;

        public StandardDataStreamSplitter(char splitter)
        {
            _splitter = splitter;
        }

        public IEnumerable<RawCommand> Split(string data)
        {
            data = MergeBuffers(data);
            data = TrimToFirstSplitter(data);
            data = TrimAfterLastSplitter(data);

            var content = data.SplitBy(_splitter)
                              .ToRawCommands();

            return new RawCommandCollection(FilterSplittedData(content));
        }

        private string MergeBuffers(string data)
        {
            if (string.IsNullOrEmpty(_buffer))
                return data;

            var output = _buffer + data;
            _buffer = string.Empty;
            return output;
        }

        private string TrimToFirstSplitter(string data)
        {
            int splIndex = data.IndexOf(_splitter);
            if (splIndex == -1)
                return string.Empty;
            return splIndex == 0 ? data : data.Substring(splIndex);
        }

        private string TrimAfterLastSplitter(string data)
        {
            int splCount = data.Count(c => c == _splitter);
            if (splCount == 1)
            {
                _buffer += data;
                return string.Empty;
            }
            else if (splCount > 1)
            {
                int splIndex = data.LastIndexOf(_splitter);
                _buffer += data.Substring(splIndex);
                return data.Substring(0, splIndex);
            }
            return string.Empty;
        }

        protected virtual IEnumerable<RawCommand> FilterSplittedData(IEnumerable<RawCommand> commands) => commands;
    }
}