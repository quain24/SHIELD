using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom.RawDataProcessing
{
    public class IncomingDataPreparer : IIncomingDataPreparer
    {
        private const int IndexNotFound = -1;
        private const int BufferStart = 0;

        private readonly int _dataCommandNumber = (int)Enums.CommandType.Data;
        private readonly int _commandTypeLength;
        private readonly int _hostIdLength;
        private readonly int _idLength;
        private readonly int _dataPackLength;
        private readonly char _separator;
        private readonly Regex _commandPattern;

        private List<string> _outputCollection = new List<string>();
        private int _patternIndex = IndexNotFound;

        private string _buffer = string.Empty;
        private string _cutoffsbuffer = string.Empty;

        internal int CommandLengthWithData { get; }

        internal int CommandLength { get; }

        public IncomingDataPreparer(int commandTypeLength, int idLength, int hostIdLength, int dataPackLength, Regex commandPattern, char separator)
        {
            _commandTypeLength = commandTypeLength;
            _idLength = idLength;
            _hostIdLength = hostIdLength;
            _dataPackLength = dataPackLength;
            _commandPattern = commandPattern;
            _separator = separator;

            CommandLength = _commandTypeLength + _idLength + 3;
            CommandLengthWithData = CommandLength + _dataPackLength; // no + 1, because there is no separator after data portion

            // TODO check everything here before modifying passed pattern that will include host id 
        }

        public List<string> DataSearch(string data)
        {
            if (CommandLengthWithData <= 0 && !string.IsNullOrEmpty(data))
                return new List<string>();

            _buffer += data;

            ResetInternalVariables();

            while (_buffer.Length > 0)
                ProcessData();

            return _outputCollection;
        }

        private void ResetInternalVariables()
        {
            _patternIndex = IndexNotFound;
            _outputCollection = new List<string>();
        }

        public void ProcessData()
        {
            MergeBuffers();
            _patternIndex = FindPatternIndexInBuffer();

            if (_patternIndex != IndexNotFound)
                ProcessAsPossibleCommand();
            else
                CleanUpBufferFromJunk();
        }

        private void MergeBuffers()
        {
            if (_cutoffsbuffer.Length > 0)
                _buffer = _cutoffsbuffer + _buffer;
            _cutoffsbuffer = string.Empty;
        }

        private int FindPatternIndexInBuffer()
        {
            Match match = _commandPattern.Match(_buffer);
            return match.Success
                ? match.Index
                : IndexNotFound;
        }

        private void ProcessAsPossibleCommand()
        {
            if (!PatternIndexInFront())
                MoveFromBufferToOutput(length: _patternIndex);

            if (IsFoundCommandADataType())
                ProcessAsDataCommand();
            else
                ProcessAsNormalCommand();
        }

        private bool IsFoundCommandADataType()
        {
            if (int.TryParse(GetCommandString(), out int result))
                return result == _dataCommandNumber;
            return false;
        }

        private void CleanUpBufferFromJunk()
        {
            int separatorIndex = FindLastSeparatorIndexInBuffer();

            if (_buffer.Length < CommandLength)
                MoveToCutoffsFromBuffer();
            else if (separatorIndex == IndexNotFound)
                MoveFromBufferToOutput();
            else
            {
                MoveFromBufferToOutput(length: separatorIndex);
                MoveToCutoffsFromBuffer();
            }
        }

        private int FindLastSeparatorIndexInBuffer() => _buffer.LastIndexOf(_separator);

        private string GetCommandString()
        {
            if ((BufferStart + 1 + _commandTypeLength) <= _buffer.Length)
                return _buffer.Substring(BufferStart + 1, _commandTypeLength);
            else
                return string.Empty;
        }

        private void ProcessAsNormalCommand() => MoveFromBufferToOutput(length: CommandLength);

        private void ProcessAsDataCommand()
        {
            int separatorIndex = SeparatorInDataPackIndex();
            if (separatorIndex > CommandLengthWithData - 1 || (separatorIndex == IndexNotFound && IsDataPackLongEnough()))
                MoveFromBufferToOutput(length: CommandLengthWithData);
            else
                HandleInvalidDataPack(separatorIndex);
        }

        private int SeparatorInDataPackIndex()
        {
            return IsDataPackLongEnough()
                ? _buffer.IndexOf(_separator, CommandLength)
                : _buffer.IndexOf(_separator, CommandLength, _buffer.Length - CommandLength);
        }

        private void HandleInvalidDataPack(int separatorIndex)
        {
            if (separatorIndex == IndexNotFound && !IsDataPackLongEnough())
                MoveToCutoffsFromBuffer();
            else
                MoveFromBufferToOutput(length: separatorIndex);
        }

        private bool PatternIndexInFront() => _patternIndex == 0;

        private bool IsDataPackLongEnough() => _buffer.Length >= CommandLengthWithData;

        private void MoveFromBufferToOutput(int index = IndexNotFound, int length = -1)
        {
            if (index < IndexNotFound) throw new System.ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} cannot be less than {nameof(IndexNotFound)} value({IndexNotFound}).");
            if (length < -1) throw new System.ArgumentOutOfRangeException(nameof(index), $"{nameof(length)} cannot be negative");

            if (index == IndexNotFound && length == -1)
            {
                AddToOutput(_buffer);
                _buffer = string.Empty;
            }
            else
            {
                index = index < 0 ? 0 : index;
                length = length < 0 ? 0 : length;
                _outputCollection.Add(_buffer.Substring(index, length));
                _buffer = _buffer.Remove(index, length);
            }
        }

        private void MoveToCutoffsFromBuffer(int index = IndexNotFound, int length = -1)
        {
            if (index < IndexNotFound) throw new System.ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} cannot be less than {nameof(IndexNotFound)} value({IndexNotFound}).");
            if (length < -1) throw new System.ArgumentOutOfRangeException(nameof(index), $"{nameof(length)} cannot be negative");

            if (index == IndexNotFound && length == -1)
            {
                _cutoffsbuffer += _buffer;
                _buffer = string.Empty;
            }
            else
            {
                index = index < 0 ? 0 : index;
                length = length < 0 ? 0 : length;
                _cutoffsbuffer += _buffer.Substring(index, length);
                _buffer = _buffer.Remove(index, length);
            }
        }

        private void AddToOutput(string data) => _outputCollection.Add(data);
    }
}