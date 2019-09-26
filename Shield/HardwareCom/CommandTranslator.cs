using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Linq;
using System.Text;

namespace Shield.HardwareCom
{
    /// <summary>
    /// Translates given CommandModel into 'string' of raw data that could be sent and vice versa.
    /// </summary>
    public class CommandTranslator : ICommandTranslator
    {
        private const char SEPARATOR = '*';
        private const char FILLER = '.';

        private IAppSettings _appSettings;
        private Func<ICommandModel> _commandModelFac;
        private IApplicationSettingsModel _appSettingsModel;

        public CommandTranslator(IAppSettings appSettings, Func<ICommandModel> commandModelFac)
        {
            _appSettings = appSettings;
            _commandModelFac = commandModelFac; // Autofac auutofactory
            _appSettingsModel = (IApplicationSettingsModel)_appSettings.GetSettingsFor(SettingsType.Application);
        }

        public ICommandModel FromString(string rawData)
        {
            ICommandModel command = _commandModelFac();
            string rawCom;
            string rawDat = string.Empty;
            string rawId = string.Empty;

            int commandSizeWithSepparators = _appSettingsModel.CommandTypeSize + 2;
            int idWithSepparator = _appSettingsModel.IdSize + 1;

            if (rawData.Length >= _appSettingsModel.CommandTypeSize + 2)
            {
                rawCom = rawData.Substring(0, commandSizeWithSepparators);    // Command in *0123* format (including asterisc or other SEPARATOR)
                rawDat = rawData.Substring(commandSizeWithSepparators + idWithSepparator); // Data starts after command type and id. Example: *0001*A8DD*12345678912345
                rawId = rawData.Substring(commandSizeWithSepparators, idWithSepparator); // Get it with sepparator

                if (rawCom.First() == SEPARATOR &&
                    rawCom.Last() == SEPARATOR)
                {
                    int rawComInt;
                    if (Int32.TryParse(rawCom.Substring(1, _appSettingsModel.CommandTypeSize), out rawComInt))
                    {
                        if (Enum.IsDefined(typeof(CommandType), rawComInt))
                            command.CommandType = (CommandType)rawComInt;
                        else
                            command.CommandType = CommandType.Unknown;
                    }
                }
            }

            // If command is still empty, then raw data was wrong - device cannot send empty, useless communication.
            if (command.CommandType == CommandType.Empty)
                command.CommandType = CommandType.Error;

            command.Id = rawId.Substring(0, _appSettingsModel.IdSize);
            command.Data = rawDat;

            return command;
        }

        /// <summary>
        /// Translates a CommandModel into a raw formatted string if given a correct command or returns empty string for error
        /// </summary>
        /// <param name="givenCommand">Command to be trasformed into raw string</param>
        /// <returns>Raw formatted string that can be understood by connected machine</returns>
        public string FromCommand(ICommandModel givenCommand)
        {
            int completeCommandSizeWithSep = _appSettingsModel.CommandTypeSize + 2 + _appSettingsModel.IdSize + 1 + _appSettingsModel.DataSize;

            if (givenCommand is null || !Enum.IsDefined(typeof(CommandType), givenCommand.CommandType))
                return null;

            StringBuilder command = new StringBuilder(SEPARATOR.ToString());

            command.Append(((int)givenCommand.CommandType).ToString().PadLeft(_appSettingsModel.CommandTypeSize, '0')).Append(SEPARATOR);
            command.Append(givenCommand.Id).Append(SEPARATOR);

            if (givenCommand.CommandType == CommandType.Data)
                command.Append(givenCommand.Data);

            if (command.Length < completeCommandSizeWithSep)
                command.Append(FILLER, completeCommandSizeWithSep - command.Length);

            return command.ToString();
        }
    }
}