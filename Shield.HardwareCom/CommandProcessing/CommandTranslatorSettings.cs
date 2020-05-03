using System;

namespace Shield.HardwareCom.CommandProcessing
{
    public class CommandTranslatorSettings
    {
        public CommandTranslatorSettings(char separator, char filler, int commandTypeLength, int idLength, int dataPackLength, int hostIdLength, string hostId)
        {
            CheckVariables(separator, filler, commandTypeLength, idLength, dataPackLength, hostIdLength, hostId);

            Separator = separator;
            Filler = filler;

            IdLength = idLength;
            CommandTypeLength = commandTypeLength;
            HostIdLength = hostIdLength;
            DataPackLength = dataPackLength;
            HostId = hostId;
        }

        public char Separator { get; }
        public char Filler { get; }
        public int IdLength { get; }
        public int CommandTypeLength { get; }
        public int DataPackSize { get; }
        public int DataPackLength { get; }
        public int HostIdLength { get; }
        public string HostId {  get; }

        /// <summary>
        /// Full command size including separators
        /// </summary>
        public int CommandSize => HostIdLength +  CommandTypeLength + IdLength + 4;

        /// <summary>
        /// Full command size including data pack and separators
        /// </summary>
        public int CommandWithDataPackSize => CommandSize + DataPackLength;

        private void CheckVariables(char separator, char filler, int commandTypeLength, int idLength, int dataPackLength, int hostIdLength, string hostId)
        {
            if (char.IsWhiteSpace(separator)) throw new ArgumentOutOfRangeException(nameof(separator), $"{nameof(separator)} cannot be a whitespace.");
            if (char.IsWhiteSpace(filler)) throw new ArgumentOutOfRangeException(nameof(filler), $"{nameof(filler)} cannot be a whitespace.");
            if(string.IsNullOrWhiteSpace(hostId)) throw new ArgumentOutOfRangeException(nameof(hostId), $"{nameof(hostId)} cannot be empty or whitespace");
            if (filler == separator) throw new ArgumentOutOfRangeException(nameof(filler), $"{nameof(filler)} and {nameof(separator)} cannot be the same.");

            if (commandTypeLength < 0) throw new ArgumentOutOfRangeException(nameof(commandTypeLength), $"{nameof(commandTypeLength)} cannot be negative");
            if (idLength < 0) throw new ArgumentOutOfRangeException(nameof(idLength), $"{nameof(idLength)} cannot be negative");
            if (dataPackLength < 0) throw new ArgumentOutOfRangeException(nameof(dataPackLength), $"{nameof(dataPackLength)} cannot be negative");
            if (hostIdLength < 0) throw new ArgumentOutOfRangeException(nameof(hostIdLength), $"{nameof(hostIdLength)} cannot be negative");
        }
    }
}