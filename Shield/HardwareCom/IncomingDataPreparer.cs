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

        private int _dataCommandNumber = (int)Enums.CommandType.Data;

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

            return Extract();
        }

        private List<string> Extract()
        {
            StringBuilder gibberishBuffer = new StringBuilder();

            void AddGibberishToOutputCollection()
            {
                if (gibberishBuffer.Length > 0)
                {
                    _outputCollection.Add(gibberishBuffer.ToString());
                    gibberishBuffer.Clear();
                }
            }

            _outputCollection = new List<string>();

            //  If there was something left from last raw data portion - add it to current buffer
            if (_cutoffs.Length > 0)
            {
                _internalBuffer.Insert(0, _cutoffs);
                _cutoffs.Clear();
            }

            while (_internalBuffer.Length > 0)
            {
                int patternIndex = FindPatternIndex(_internalBuffer.ToString());

                //  Found pattern
                if (patternIndex >= 0)
                {
                    int ealierSeparatorIndex = _internalBuffer.ToString().IndexOf(Separator);
                    if (ealierSeparatorIndex < patternIndex)
                    {
                        gibberishBuffer.Append(_internalBuffer.ToString(0, patternIndex - 1));
                    }

                    //  If its data type
                    if (int.Parse(_internalBuffer.ToString(patternIndex + 1, CommandTypeLength)) == _dataCommandNumber)
                    {
                        //  Is there enough chars to fill data portion of command?
                        if (_internalBuffer.Length >= CommandLengthWithData + patternIndex)
                        {
                            //  check for preliminary correctness of raw data pack
                            int isThereSeparatorInData = FindSeparatorIndex(_internalBuffer.ToString(CommandLength, DataPackLength));

                            // Data pack is busted, so throw it into return, recipient will handle this
                            if (isThereSeparatorInData >= 0)
                            {
                                AddGibberishToOutputCollection();
                                _outputCollection.Add(_internalBuffer.ToString(patternIndex, CommandLength + isThereSeparatorInData));
                                _internalBuffer.Remove(0, CommandLength + isThereSeparatorInData);
                                continue;
                            }

                            //  Correct data pack!!
                            else
                            {
                                AddGibberishToOutputCollection();
                                _outputCollection.Add(_internalBuffer.ToString(patternIndex, CommandLengthWithData));
                                _internalBuffer.Remove(0, patternIndex + CommandLengthWithData);
                                continue;
                            }
                        }

                        //  Data pack is incomplete - lets leave it for next raw data portion
                        else
                        {
                            _cutoffs.Append(_internalBuffer);
                            _internalBuffer.Clear();
                            continue;
                        }
                    }

                    //  If its not data, then just add it to return
                    else
                    {
                        AddGibberishToOutputCollection();
                        _outputCollection.Add(_internalBuffer.ToString(patternIndex, CommandLength));
                        _internalBuffer.Remove(0, patternIndex + CommandLength);
                        continue;
                    }
                }

                //  No pattern, no data before, lets try to find a separator at least
                else
                {
                    int separatorIndex = FindSeparatorIndex(_internalBuffer.ToString());
                    //  separator found, but not as one and only char in buffer
                    if (separatorIndex >= 0 && _internalBuffer.Length > 1)
                    {
                        _cutoffs.Append(_internalBuffer.ToString(separatorIndex, _internalBuffer.Length - separatorIndex));
                        gibberishBuffer.Append(_internalBuffer.ToString(0, separatorIndex));
                        _internalBuffer.Remove(0, _internalBuffer.Length - separatorIndex);
                    }

                    // There is just a separator in buffer
                    else if (separatorIndex >= 0)
                    {
                        _cutoffs.Append(Separator);
                        _internalBuffer.Clear();
                    }
                    //  Nothing found
                    else
                    {
                        gibberishBuffer.Append(_internalBuffer.ToString());
                        _internalBuffer.Clear();
                    }
                }
            }

            AddGibberishToOutputCollection();
            return _outputCollection;
        }

        private int FindPatternIndex(string data)
        {
            Match match = CommandPattern.Match(data);
            if (match.Success)
                return match.Index;
            else
                return -1;
        }

        private int FindSeparatorIndex(string data, int startIndex = 0, int count = 0)
        {
            if (count == 0)
                return data.IndexOf(Separator, startIndex);
            else
                return data.IndexOf(Separator, startIndex, count);
        }
    }
}