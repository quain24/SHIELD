using Shield.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.RawData
{
    public class DataStreamSplitter : IDataStreamSplitter
    {
        private readonly char _splitter;
        private readonly HashSet<int> _allowedLengths;
        private string _cutoff = string.Empty;
        private string _buffer = string.Empty;

        public DataStreamSplitter(char splitter, params int[] allowedLengths)
        {
            if (allowedLengths.IsNullOrEmpty() || allowedLengths.Any(val => val <= 0))
                throw new ArgumentException($"{nameof(allowedLengths)} must contain only positive values and cannot be empty or null.");

            _splitter = splitter;
            _allowedLengths = allowedLengths.ToHashSet();
        }

        public IEnumerable<string> Split(string data)
        {
            data = MergeBuffers(data);
            data = TrimToFirstSplitter(data);
            data = TrimAfterLastSplitter(data);

            var content = data.SplitBy(_splitter).ToList();

            return new RawCommandCollection(content.Where(command => _allowedLengths.Any(length => length == command.Length)));
        }

        private string MergeBuffers(string data)
        {
            if (string.IsNullOrEmpty(_cutoff))
                return data;

            var output = _cutoff + data;
            _cutoff = string.Empty;
            return output;
        }

        private string TrimToFirstSplitter(string data)
        {
            int splIndex = data.IndexOf(_splitter);
            if (splIndex == -1)
                return string.Empty;
            return data.Substring(splIndex);
        }

        private string TrimAfterLastSplitter(string data)
        {
            int splCount = data.Count(c => c == _splitter);
            if (splCount == 1)
            {
                _cutoff += data;
                return string.Empty;
            }
            else if (splCount > 1)
            {
                int splIndex = data.LastIndexOf(_splitter);
                _cutoff += data.Substring(splIndex);
                return data.Substring(0, splIndex);
            }
            return string.Empty;
        }

        private string DataPrep(string data)
        {
            int splitterIndex = data.LastIndexOf(_splitter);
            if (splitterIndex >= 0 && splitterIndex < data.Length - 1)
            {
                _cutoff = data.Substring(splitterIndex);
                return data.Substring(0, splitterIndex);
            }
            _cutoff = string.Empty;
            return data;
        }

        private string PrepareData(string data)
        {
            data = _cutoff + data;
            _cutoff = string.Empty;
            if (data.FirstOrDefault() == _splitter)
                return data;
            int splitIndex = data.IndexOf(_splitter);
            if (splitIndex == -1)
                return string.Empty;
            return data.Substring(splitIndex);
        }

        private bool BufferHasSplitter() => _buffer.FirstOrDefault() == _splitter;

        private void CreateNewCutoffFrom(List<string> content)
        {
            _cutoff = content.LastOrDefault() is string cutoff && cutoff.Length < _allowedLengths.Max()
                ? _splitter + cutoff
                : string.Empty;
        }

        private void MergeCutoffAndBuffer()
        {
        }
    }
}