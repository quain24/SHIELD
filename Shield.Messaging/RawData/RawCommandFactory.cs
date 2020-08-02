using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Extensions;
using System;
using System.Text;

namespace Shield.Messaging.RawData
{
    public class RawCommandFactory
    {
        private readonly char _splitter;
        private readonly char _separator;

        public RawCommandFactory(char splitter, char separator)
        {
            ValidateInParameters(splitter, separator);
            _splitter = splitter;
            _separator = separator;
        }

        private void ValidateInParameters(char splitter, char separator)
        {
            var text = "cannot be empty, same as other parameter or whitespace";
            if (splitter is default(char) || splitter == ' ' || separator == splitter)
                throw new ArgumentOutOfRangeException(nameof(splitter), $"{nameof(splitter)} {text}");
            if (separator is default(char) || separator == ' ')
                throw new ArgumentOutOfRangeException(nameof(separator), $"{nameof(separator)} {text}");
        }

        public RawCommand TranslateFrom(ICommand command)
        {
            var rawData = new StringBuilder();
            rawData.Append(_splitter);

            foreach (var part in command)
                if (!(part is EmptyPart))
                    rawData.Append(RawPartWithSeparator(part));

            return rawData.Append(_splitter)
                .ToString()
                .ToRawCommand();
        }

        private string RawPartWithSeparator(IPart part) => _separator + part.ToString();
    }
}