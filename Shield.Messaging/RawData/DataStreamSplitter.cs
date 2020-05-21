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

        public DataStreamSplitter(char splitter, params int[] allowedLengths)
        {
            if (allowedLengths.IsNullOrEmpty() || allowedLengths.Any(val => val <= 0))
                throw new ArgumentException($"{nameof(allowedLengths)} must contain only positive values and cannot be empty or null.");

            _splitter = splitter;
            _allowedLengths = allowedLengths.ToHashSet();
        }

        public IEnumerable<string> Split(string data)
        {
            data = _cutoff + data;
            var content = data.SplitBy(_splitter).ToList();
            var rawCommands = new RawCommandCollection(content.Where(command => _allowedLengths.Any(length => length == command.Length)));

            if (content.Count > rawCommands.Count)
                CreateNewCutoffFrom(content);
            return rawCommands;
        }

        private void CreateNewCutoffFrom(List<string> content)
        {
            _cutoff = content.LastOrDefault() is string cutoff && cutoff.Length < _allowedLengths.Max()
                ? cutoff
                : string.Empty;
        }
    }
}