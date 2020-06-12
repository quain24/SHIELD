using Shield.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.RawData
{
    public class AllowedLengthsDataStreamSplitter : StandardDataStreamSplitter
    {
        private readonly HashSet<int> _allowedLengths;

        public AllowedLengthsDataStreamSplitter(char splitter, params int[] allowedLengths)
            : base(splitter)
        {
            if (allowedLengths.IsNullOrEmpty() || allowedLengths.Any(val => val <= 0))
                throw new ArgumentException($"{nameof(allowedLengths)} must contain only positive values and cannot be empty or null.");

            _allowedLengths = allowedLengths.ToHashSet();
        }

        override protected IEnumerable<RawCommand> FilterSplittedData(IEnumerable<RawCommand> commands) =>
            RawDataOfAllowedLengthsFrom(commands);

        private IEnumerable<RawCommand> RawDataOfAllowedLengthsFrom(IEnumerable<RawCommand> content) =>
            content.Where(command => _allowedLengths.Any(length => length == command.Length));
    }
}