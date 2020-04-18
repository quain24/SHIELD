using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom.RawDataProcessing
{

    /// <summary>
    /// DEPRECEATED - use IncomingDataPreparer
    /// </summary>

    public class IncomingDataPreparerOld : IIncomingDataPreparer
    {
        private StringBuilder _internalBuffer = new StringBuilder();
        private StringBuilder _cutoffs = new StringBuilder();

        private readonly int _commandTypeLength;
        private readonly int _idLength;
        private readonly int _dataPackLength;
        private readonly char _separator;
        private readonly Regex _commandPattern;
        private List<string> _outputCollection;

        private readonly int _dataCommandNumber = (int)Enums.CommandType.Data;

        internal int CommandLengthWithData
        {
            get
            {
                if (_commandTypeLength > 0 && _idLength > 0 && _dataPackLength > 0)
                    return _commandTypeLength + _idLength + _dataPackLength + 3;
                else
                    return -1;
            }
        }

        internal int CommandLength
        {
            get
            {
                if (_commandTypeLength > 0 && _idLength > 0)
                    return _commandTypeLength + _idLength + 3;
                else
                    return -1;
            }
        }

        public IncomingDataPreparerOld(int commandTypeLength, int idLength, int dataPackLength, Regex commandPattern, char separator)
        {
            _commandTypeLength = commandTypeLength;
            _idLength = idLength;
            _dataPackLength = dataPackLength;
            _commandPattern = commandPattern;
            _separator = separator;
        }

        public List<string> DataSearch(string data)
        {
            if (CommandLengthWithData <= 0 && !string.IsNullOrEmpty(data))
                return new List<string>();

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
                    int ealier_separatorIndex = _internalBuffer.ToString().IndexOf(_separator);
                    if (ealier_separatorIndex < patternIndex)
                    {
                        gibberishBuffer.Append(_internalBuffer.ToString(0, patternIndex - 1));
                    }

                    //  If its data type
                    if (int.Parse(_internalBuffer.ToString(patternIndex + 1, _commandTypeLength)) == _dataCommandNumber)
                    {
                        //  Is there enough chars to fill data portion of command?
                        if (_internalBuffer.Length >= CommandLengthWithData + patternIndex)
                        {
                            //  check for preliminary correctness of raw data pack
                            int isThere_separatorInData = Find_separatorIndex(_internalBuffer.ToString(CommandLength, _dataPackLength));

                            // Data pack is busted, so throw it into return, recipient will handle this
                            if (isThere_separatorInData >= 0)
                            {
                                AddGibberishToOutputCollection();
                                _outputCollection.Add(_internalBuffer.ToString(patternIndex, CommandLength + isThere_separatorInData));
                                _internalBuffer.Remove(0, CommandLength + isThere_separatorInData);
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
                    int separatorIndex = Find_separatorIndex(_internalBuffer.ToString());
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
                        _cutoffs.Append(_separator);
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
            Match match = _commandPattern.Match(data);
            if (match.Success)
                return match.Index;
            else
                return -1;
        }

        private int Find_separatorIndex(string data, int startIndex = 0, int count = 0)
        {
            if (count == 0)
                return data.IndexOf(_separator, startIndex);
            else
                return data.IndexOf(_separator, startIndex, count);
        }
    }
}