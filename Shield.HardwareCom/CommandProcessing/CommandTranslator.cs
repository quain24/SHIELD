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
    public class CommandTranslator : ICommandTranslator
    {
        private readonly CommandTranslatorSettings _settings;
        private readonly ICommandModelFactory _factory;
        private string _idFiller;

        public CommandTranslator(CommandTranslatorSettings settings, ICommandModelFactory factory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            GenerateIDFiller();
        }

        private string GenerateIDFiller() => _idFiller = _settings.Filler.ToString().PadLeft(_settings.IdLength, _settings.Filler);

        public ICommandModel FromString(string rawData)
        {
            if (rawData is null) throw new ArgumentNullException(nameof(rawData), "Cannot create command from NULL.");

            if (IsOfProperLength(rawData))
                return CreateFromValidLengthRawData(rawData);
            return CreateFromInvalidRawData(rawData);
        }

        private bool IsOfProperLength(string data) => data.Length == _settings.CommandSize || data.Length == _settings.CommandWithDataPackSize;

        private ICommandModel CreateFromValidLengthRawData(string data)
        {
            var type = GetCommandTypeValue(data);
            var id = GetID(type, data);
            var dataPack = GetDataPack(type, data);

            return _factory.Create(type, id, Timestamp.TimestampNow, dataPack);
        }

        private ICommandModel CreateFromInvalidRawData(string data)
        {
            return _factory.Create(CommandType.Error, _idFiller, Timestamp.TimestampNow, ParseErrorDataPack(data));
        }

        private CommandType GetCommandTypeValue(string data)
        {
            if (int.TryParse(data.Substring(1, _settings.CommandTypeLength), out int value))
                return IsATypeOfCommand(value) ? (CommandType)value : CommandType.Unknown;
            return CommandType.Error;
        }

        private bool IsATypeOfCommand(int type) => Enum.IsDefined(typeof(CommandType), type);

        private bool IsKnownTypeOfCommand(ICommandModel command) => IsATypeOfCommand((int)command.CommandType);

        private string GetID(CommandType type, string data) => type == CommandType.Error ? _idFiller : ParseId(data);

        private string GetID(ICommandModel command) => command.Id;

        private string ParseId(string data) => data.Substring(_settings.CommandTypeLength + 2, _settings.IdLength);

        private string GetDataPack(CommandType type, string data)
        {
            if (type == CommandType.Error)
                return ParseErrorDataPack(data);
            else
                return ParseGoodDataPack(data);
        }

        private string GetDataPack(ICommandModel command) => command.Data.PadLeft(_settings.DataPackLength, _settings.Filler);

        private string ParseErrorDataPack(string data)
        {
            return data.Length > _settings.DataPackLength
                ? data.Substring(0, _settings.DataPackLength)
                : data.PadLeft(_settings.DataPackLength, _settings.Filler);
        }

        private string ParseGoodDataPack(string data) => data.Substring(_settings.CommandSize);

        /// <summary>
        /// Translates a CommandModel into a raw formatted string if given a correct command or returns empty string for error
        /// </summary>
        /// <param name="givenCommand">Command to be transformed into raw string</param>
        /// <returns>Raw formatted string that can be understood by connected machine</returns>
        public string FromCommand(ICommandModel givenCommand)
        {
            if (givenCommand is null) throw new ArgumentNullException(nameof(givenCommand), "Cannot create raw command string from NULL.");

            if (!IsKnownTypeOfCommand(givenCommand))
                return string.Empty;

            var command = new StringBuilder(_settings.Separator.ToString());

            command.Append(GetCommandType(givenCommand)).Append(_settings.Separator);
            command.Append(GetID(givenCommand)).Append(_settings.Separator);

            if (IsCommandADataType(givenCommand))
                command.Append(GetDataPack(givenCommand));

            return command.ToString();
        }

        private string GetCommandType(ICommandModel command) => ((int)command.CommandType).ToString().ToUpperInvariant().PadLeft(_settings.CommandTypeLength, '0');

        private bool IsCommandADataType(ICommandModel command) => command.CommandType == CommandType.Data;
    }
}