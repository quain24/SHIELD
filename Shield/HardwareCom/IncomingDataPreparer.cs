using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom
{
    public class IncomingDataPreparer : IIncomingDataPreparer
    {
        private StringBuilder _internalBuffer = new StringBuilder();
        private int _commandTypeLength = -1;
        private int _idLength = -1;
        private int _dataLength = -1;
        private Regex _commandPattern;
        private List<string> _outputCollection;

        public int CommandTypeLength { get { return _commandTypeLength; } set { _commandTypeLength = value > 0 ? value : 0; } }
        public int IDLength { get { return _idLength; } set { _idLength = value > 0 ? value : 0; } }
        public int DataPackLength { get { return _dataLength; } set { _dataLength = value > 0 ? value : 0; } }
        public Regex CommandPattern { get => _commandPattern; set => _commandPattern = value; }
        public char Separator { get; set; }

        public int CommandLength
        {
            get
            {
                if (CommandTypeLength > 0 && IDLength > 0 && DataPackLength > 0)
                    return CommandTypeLength + IDLength + DataPackLength + 3;
                else
                    return -1;
            }
        }

        //public IncomingDataPreparer(){ }

        public IncomingDataPreparer(int commandTypeLength, int idLength, int dataPackLength, Regex commandPattern, char separator)
        {
            _commandTypeLength = commandTypeLength;
            _idLength = idLength;
            _dataLength = dataPackLength;
            _commandPattern = commandPattern;
            Separator = separator;
        }

        public List<string> DataSearch(string data)
        {
            if (CommandLength < 0 && !string.IsNullOrEmpty(data))
                return null;

            _internalBuffer.Append(data);

            return Extract();
        }

        private List<string> Extract()
        {
            _outputCollection = new List<string>();

            while (_internalBuffer.Length >= CommandLength)
            {
                int correctDataIndex = CheckRawData(_internalBuffer.ToString(0, CommandLength));

                // Partially bad data
                if (correctDataIndex > 0)
                {
                    _outputCollection.Add(_internalBuffer.ToString(0, correctDataIndex));
                    _internalBuffer.Remove(0, correctDataIndex);
                }

                // All good or all bad
                else
                {
                    _outputCollection.Add(_internalBuffer.ToString(0, CommandLength));
                    _internalBuffer.Remove(0, CommandLength);
                }
            }

            return _outputCollection;
        }

        private int CheckRawData(string data)
        {
            Match match = CommandPattern.Match(data);
            if (match.Success)
                return match.Index;

            int firsIndexOfSepprarator = data.IndexOf(Separator);

            if (firsIndexOfSepprarator == -1)
                return firsIndexOfSepprarator;

            if (firsIndexOfSepprarator == 0)
                return 1;
            else
                return firsIndexOfSepprarator;
        }
    }
}