using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Text;

namespace Shield.HardwareCom.CommandProcessing
{
    /// <summary>
    /// Translates given CommandModel into 'string' of raw data that could be sent and vice versa.
    /// </summary>
    public class CommandTranslator : ICommandTranslator
    {
        private readonly int _commandLengthWithData;
        private readonly int _commandLength;
        private readonly char _separator;
        private readonly char _filler;

        private readonly Func<ICommandModel> _commandModelFac;
        private readonly IApplicationSettingsModel _appSettingsModel;

        public CommandTranslator(IApplicationSettingsModel applicationSettings, Func<ICommandModel> commandModelFac)
        {
            _commandModelFac = commandModelFac ?? throw new ArgumentNullException(nameof(commandModelFac)); // Autofac factory
            _appSettingsModel = applicationSettings ?? throw new ArgumentNullException(nameof(applicationSettings));

            _separator = _appSettingsModel.Separator;
            _filler = _appSettingsModel.Filler;
            _commandLengthWithData = _appSettingsModel.CommandTypeSize + _appSettingsModel.IdSize + _appSettingsModel.DataSize + 3;
            _commandLength = _appSettingsModel.CommandTypeSize + _appSettingsModel.IdSize + 3;
        }

        public ICommandModel FromString(string rawData)
        {
            ICommandModel command = _commandModelFac();
            string rawCommandTypeString = string.Empty;
            string rawDataString = string.Empty;
            string rawIdString = string.Empty;

            if (rawData.Length == _commandLengthWithData || rawData.Length == _commandLength)
            {
                rawCommandTypeString = rawData.Substring(1, _appSettingsModel.CommandTypeSize);
                rawIdString = rawData.Substring(2 + _appSettingsModel.CommandTypeSize, _appSettingsModel.IdSize);

                if (rawData.Length == _commandLengthWithData)
                    rawDataString = rawData.Substring(3 + _appSettingsModel.CommandTypeSize + _appSettingsModel.IdSize);

                int rawComInt;
                if (int.TryParse(rawCommandTypeString, out rawComInt))
                {
                    if (Enum.IsDefined(typeof(CommandType), rawComInt))
                        command.CommandType = (CommandType)rawComInt;
                    else
                        command.CommandType = CommandType.Unknown;

                    command.Id = rawIdString;
                    command.Data = rawData.Length == _commandLengthWithData ? rawDataString : string.Empty;
                }
                else
                {
                    command.CommandType = CommandType.Error;
                    command.Id = string.Empty.PadLeft(_appSettingsModel.IdSize, _filler);
                    command.Data = rawData.Length > _appSettingsModel.DataSize ? rawData.Substring(0, _appSettingsModel.DataSize) : rawData;
                }
            }
            else
            {
                command.CommandType = CommandType.Error;
                command.Id = string.Empty.PadLeft(_appSettingsModel.IdSize, _filler);
                command.Data = rawData.Length > _appSettingsModel.DataSize ? rawData.Substring(0, _appSettingsModel.DataSize) : rawData;
            }

            command.TimeStamp = Timestamp.TimestampNow;
            return command;
        }

        /// <summary>
        /// Translates a CommandModel into a raw formatted string if given a correct command or returns empty string for error
        /// </summary>
        /// <param name="givenCommand">Command to be transformed into raw string</param>
        /// <returns>Raw formatted string that can be understood by connected machine</returns>
        public string FromCommand(ICommandModel givenCommand)
        {
            int completeCommandSizeWithSep = _appSettingsModel.CommandTypeSize + 2 + _appSettingsModel.IdSize + 1 + _appSettingsModel.DataSize;

            if (givenCommand is null || !Enum.IsDefined(typeof(CommandType), givenCommand.CommandType))
                return null;

            StringBuilder command = new StringBuilder(_separator.ToString());

            command.Append(((int)givenCommand.CommandType).ToString().ToUpperInvariant().PadLeft(_appSettingsModel.CommandTypeSize, '0')).Append(_separator);
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