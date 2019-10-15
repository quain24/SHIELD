using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom
{
    public class IncomingDataPreparer : IIncomingDataPreparer
    {
        private StringBuilder _internalBuffer = new StringBuilder();
        private StringBuilder _cutoffs = new StringBuilder();
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

        public int CommandLengthWithData
        {
            get
            {
                if (CommandTypeLength > 0 && IDLength > 0 && DataPackLength > 0)
                    return CommandTypeLength + IDLength + DataPackLength + 3;
                else
                    return -1;
            }
        }

        public int CommandLength
        {
            get
            {
                if (CommandTypeLength > 0 && IDLength > 0)
                    return CommandTypeLength + IDLength + 3;
                else
                    return -1;
            }
        }

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
            if (CommandLengthWithData <= 0 && !string.IsNullOrEmpty(data))
                return null;

            _internalBuffer.Append(data);

            if (_internalBuffer.Length > CommandLength)
                return Extract();
            return null;
        }

        private List<string> Extract()
        {
            _outputCollection = new List<string>();
            StringBuilder gibberishBuffer = new StringBuilder();
            int dataCommandNumber = (int)Enums.CommandType.Data;
            int errorCommandNumber = (int)Enums.CommandType.Error;

            while (_internalBuffer.Length >= CommandLength)
            {
                // lets not allow trash buffer to overgrow, right?
                if (gibberishBuffer.Length > CommandLengthWithData * 10)
                    gibberishBuffer.Remove(0, CommandLengthWithData);

                int correctDataIndex = CheckRawData(_internalBuffer.ToString(0, CommandLength));

                // Partially bad data
                if (correctDataIndex > 0)
                {
                    gibberishBuffer.Append(_internalBuffer.ToString(0, correctDataIndex));
                    _internalBuffer.Remove(0, correctDataIndex);
                    continue;
                }
                // Completely bad data
                else if (correctDataIndex < 0)
                {
                    gibberishBuffer.Append(_internalBuffer.ToString(0, CommandLength));
                    _internalBuffer.Remove(0, CommandLength);
                    continue;
                }

                // Found good data, but if its of 'data' type additional checks will be performed
                else
                {
                    int prelimenaryCommandType;
                    if (!int.TryParse(_internalBuffer.ToString(1, CommandTypeLength), out prelimenaryCommandType))
                        prelimenaryCommandType = errorCommandNumber;

                    // Before adding any good commands to output lets add garbage collected ealier,
                    // if there is any, for recepient to handle
                    if (gibberishBuffer.Length > 0)
                    {
                        _outputCollection.Add(gibberishBuffer.ToString());
                        gibberishBuffer.Clear();
                    }

                    // Found data type
                    if (prelimenaryCommandType == dataCommandNumber)
                    {
                        if (_internalBuffer.Length >= CommandLengthWithData)
                        {
                            int separatorInData = _internalBuffer.ToString(CommandLength, DataPackLength).IndexOf(Separator);
                            if (separatorInData == -1)
                            {
                                _outputCollection.Add(_internalBuffer.ToString(0, CommandLengthWithData));
                                _internalBuffer.Remove(0, CommandLengthWithData);
                                continue;
                            }
                            else
                            {
                                _outputCollection.Add(_internalBuffer.ToString(0, separatorInData));
                                _internalBuffer.Remove(0, separatorInData);
                                continue;
                            }
                        }
                        else
                        {
                            int separatorInData = _internalBuffer.ToString(CommandLength, _internalBuffer.Length - CommandLength).IndexOf(Separator);
                            if (separatorInData == -1)
                            {
                                break;
                            }
                            else
                            {
                                _outputCollection.Add(_internalBuffer.ToString(0, separatorInData));
                                _internalBuffer.Remove(0, separatorInData);
                                continue;
                            }
                        }
                    }

                    // Found any other type
                    else
                    {
                        _outputCollection.Add(_internalBuffer.ToString(0, CommandLength));
                        _internalBuffer.Remove(0, CommandLength);
                    }
                }
            }

            if (gibberishBuffer.Length > 0)
            {
                _outputCollection.Add(gibberishBuffer.ToString());
                gibberishBuffer.Clear();
            }
            return _outputCollection;
        }

        private int CheckRawData(string data, bool includePattern = true)
        {
            if (includePattern)
            {
                Match match = CommandPattern.Match(data);
                if (match.Success)
                    return match.Index;
            }

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