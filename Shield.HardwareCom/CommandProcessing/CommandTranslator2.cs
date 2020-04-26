using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Helpers;
using Shield.HardwareCom.Models;
using System;
using System.Text;

namespace Shield.HardwareCom.CommandProcessing
{
    /// <summary>
    /// Translates given CommandModel into 'string' of raw data that could be sent and vice versa.
    /// </summary>
    public class CommandTranslator2 : ICommandTranslator
    {
        private readonly int _commandLengthWithData;
        private readonly int _commandLength;
        private readonly char _separator;
        private readonly char _filler;

        private readonly CommandTranslatorSettings _settings;
        private readonly ICommandModelFactory _factory;
        private string _idFiller;
        private string _workpiece = string.Empty;

        public CommandTranslator2(CommandTranslatorSettings settings, ICommandModelFactory factory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _separator = _settings.Separator;
            _filler = _settings.Filler;
            _commandLengthWithData = _settings.CommandWithDataPackSize;
            _commandLength = _settings.CommandSize;

            GenerateIDFiller();
        }

        private string GenerateIDFiller() => _idFiller = _settings.Filler.ToString().PadLeft(_settings.IdLength, _settings.Filler);

        public ICommandModel FromString(string rawData)
        {
            if (rawData is null) throw new ArgumentNullException(nameof(rawData), "Cannot create command from NULL.");

            AssignWorpiece(rawData);

            if (IsOfProperLength(rawData))
                return CreateFromValidLengthRawData();
            return CreateFromInvalidRawData();
        }

        private void AssignWorpiece(string rawData) => _workpiece = rawData;

        private bool IsOfProperLength(string raw) => raw.Length == _commandLength || raw.Length == _commandLengthWithData;

        private ICommandModel CreateFromValidLengthRawData()
        {
            var type = GetCommandTypeValue();
            var id = GetID(type);
            var dataPack = GetDataPack(type);

            return _factory.Create(type, id, Timestamp.TimestampNow, dataPack);
        }

        private ICommandModel CreateFromInvalidRawData()
        {
            return _factory.Create(CommandType.Error, _idFiller, Timestamp.TimestampNow, ParseErrorDataPack());
        }

        private CommandType GetCommandTypeValue()
        {
            if (int.TryParse(_workpiece.Substring(1, _settings.CommandTypeLength), out int val))
                return Enum.IsDefined(typeof(CommandType), val) ? (CommandType)val : CommandType.Unknown;
            return CommandType.Error;
        }

        private string GetID(CommandType type) => type == CommandType.Error ? _idFiller : ParseId();

        private string ParseId() => _workpiece.Substring(_settings.CommandTypeLength + 2, _settings.IdLength);

        private string GetDataPack(CommandType type)
        {
            if (type == CommandType.Error)
                return ParseErrorDataPack();
            else
                return ParseGoodDataPack();
        }

        private string ParseErrorDataPack()
        {
            return _workpiece.Length > _settings.DataPackLength
                ? _workpiece.Substring(0, _settings.DataPackLength)
                : _workpiece.PadLeft(_settings.DataPackLength, _settings.Filler);
        }

        private string ParseGoodDataPack() => _workpiece.Substring(_settings.CommandSize);


        // TODO refactor this method
        /// <summary>
        /// Translates a CommandModel into a raw formatted string if given a correct command or returns empty string for error
        /// </summary>
        /// <param name="givenCommand">Command to be transformed into raw string</param>
        /// <returns>Raw formatted string that can be understood by connected machine</returns>
        public string FromCommand(ICommandModel givenCommand)
        {
            int completeCommandSizeWithSep = _settings.CommandWithDataPackSize;

            if (givenCommand is null || !Enum.IsDefined(typeof(CommandType), givenCommand.CommandType))
                return null;

            StringBuilder command = new StringBuilder(_separator.ToString());

            command.Append(((int)givenCommand.CommandType).ToString().ToUpperInvariant().PadLeft(_settings.CommandTypeLength, '0')).Append(_separator);
            command.Append(givenCommand.Id).Append(_separator);

            if (givenCommand.CommandType == CommandType.Data)
            {
                command.Append(givenCommand.Data);
                command.Append(_filler, completeCommandSizeWithSep - command.Length);
            }

            return command.ToString();
        }
    }
}