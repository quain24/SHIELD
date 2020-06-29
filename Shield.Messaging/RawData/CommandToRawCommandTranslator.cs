using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Extensions;
using System;
using System.Text;

namespace Shield.Messaging.RawData
{
    public class CommandToRawCommandTranslator
    {
        private readonly char _splitter;
        private readonly char _separator;

        public CommandToRawCommandTranslator(char splitter, char separator)
        {
            ValidateInParameters(splitter, separator);
            _splitter = splitter;
            _separator = separator;
        }

        private void ValidateInParameters(char splitter, char separator)
        {
            var text = "cannot be empty, same as other parameter or whitespace";
            if (splitter is default(Char) || splitter == ' ' || separator == splitter)
                throw new ArgumentOutOfRangeException(nameof(splitter), $"{nameof(splitter)} {text}");
            if (separator is default(Char) || separator == ' ')
                throw new ArgumentOutOfRangeException(nameof(separator), $"{nameof(separator)} {text}");
        }

        public RawCommand TranslateFrom(ICommand command)
        {
            var rawData = new StringBuilder();
            rawData.Append(_splitter);

            rawData.Append(command.ID)
                .Append(RawPartWithSeparator(command.HostID))
                .Append(RawPartWithSeparator(command.Target))
                .Append(RawPartWithSeparator(command.Order));

            if (!(command.Data is EmptyPart))
                rawData.Append(command.Data);
            return rawData.Append(_splitter)
                .ToString()
                .ToRawCommand();
        }

        private string RawPartWithSeparator(IPart part) => _separator + part.ToString();
    }
}