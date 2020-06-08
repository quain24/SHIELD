using Shield.Extensions;
using Shield.Messaging.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.RawData
{
    public class DataStreamSplitter : IDataStreamSplitter
    {
        private readonly char _splitter;
        private readonly HashSet<int> _allowedLengths;
        private string _buffer = string.Empty;

        // TODO Work with this - remove allowed lengths - it will just split raw data stream, commands can have different lengths now

        public DataStreamSplitter(char splitter, params int[] allowedLengths)
        {
            if (allowedLengths.IsNullOrEmpty() || allowedLengths.Any(val => val <= 0))
                throw new ArgumentException($"{nameof(allowedLengths)} must contain only positive values and cannot be empty or null.");

            _splitter = splitter;
            _allowedLengths = allowedLengths.ToHashSet();
        }

        public IEnumerable<RawCommand> Split(string data)
        {
            data = MergeBuffers(data);
            data = TrimToFirstSplitter(data);
            data = TrimAfterLastSplitter(data);

            var content = data
                .SplitBy(_splitter)
                .ToRawCommands();

            return new RawCommandCollection(RawDataOfAllowedLengthsFrom(content));
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
            if (splIndex == 0)
                return data;
            return data.Substring(splIndex);
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

        private IEnumerable<RawCommand> RawDataOfAllowedLengthsFrom(IEnumerable<RawCommand> content) =>
            content.Where(command => _allowedLengths.Any(length => length == command.Length));
    }
}