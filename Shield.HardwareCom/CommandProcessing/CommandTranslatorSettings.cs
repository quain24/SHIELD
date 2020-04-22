using System;
using System.IO;

namespace Shield.HardwareCom.CommandProcessing
{
    public class CommandTranslatorSettings
    {
        public CommandTranslatorSettings(char separator, char filler, int commandlength, int commandWithDataPackLength)
        {
            CheckVariables(separator, filler, commandlength, commandWithDataPackLength);

            Separator = separator;
            Filler = filler;
            CommandSize = commandlength;
            CommandWithDataPackSize = commandWithDataPackLength;
            DataPackSize = commandWithDataPackLength - commandlength;
        }

        // TODO - replace appsettings in commandtranslator with this

        public char Separator { get; }
        public char Filler { get; }
        public int CommandSize { get; }
        public int CommandWithDataPackSize { get; }
        public int DataPackSize { get; }

        private void CheckVariables(char separator, char filler, int commandlength, int commandWithDataPackLength)
        {
            if (char.IsWhiteSpace(separator)) throw new ArgumentOutOfRangeException(nameof(separator), $"{nameof(separator)} cannot be a whitespace.");
            if (char.IsWhiteSpace(filler)) throw new ArgumentOutOfRangeException(nameof(filler), $"{nameof(filler)} cannot be a whitespace.");
            if (filler == separator) throw new ArgumentOutOfRangeException(nameof(filler), $"{nameof(filler)} and {nameof(separator)} cannot be the same.");
            if (commandlength < 0 || commandlength >= commandWithDataPackLength) throw new ArgumentOutOfRangeException(nameof(commandlength), $"{nameof(commandlength)} cannot be negative or >= than {nameof(commandWithDataPackLength)}");
            if (commandWithDataPackLength < 0) throw new ArgumentOutOfRangeException(nameof(commandWithDataPackLength), $"{nameof(commandWithDataPackLength)} cannot be negative.");
        }
    }
}