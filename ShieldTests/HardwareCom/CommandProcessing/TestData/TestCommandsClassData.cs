using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using Xunit;

namespace ShieldTests.HardwareCom.CommandProcessing.TestData
{
    public class TestCommandsClassData : TheoryData<ICommandModel, string>
    {
        private readonly DefaultConfigForCommandCreation _settings = new DefaultConfigForCommandCreation();
        private readonly CommandModelFactory _commandFactory = new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel()));
        private readonly string _dataPack = "abcdefghijklmnopqrstuvwxyz1234567890";

        public TestCommandsClassData() => GenerateCommands();

        private int[] CommandTypes => (int[])Enum.GetValues(typeof(CommandType));

        private void GenerateCommands()
        {
            foreach (var c in CommandTypes)
                GenerateCommand(c);
        }

        private void GenerateCommand(int commandEnumValue)
        {
            // Ignoring Error Command value - this class tests proper values
            if ((CommandType)commandEnumValue == CommandType.Error)
                return;
            string data;
            string commandType = GetCommandTypeString(commandEnumValue);
            string id = commandType; // The same value for simplification

            var command = _commandFactory.Create((CommandType)commandEnumValue, id);
            var commandString = _settings.Separator + commandType + _settings.Separator + id + _settings.Separator;

            if (commandEnumValue == (int)CommandType.Data)
            {
                data = GenerateDataPack();
                command.Data = data;
                commandString += data;
            }

            Add(command, commandString);
        }

        private string GenerateDataPack()
        {
            var data = _dataPack;
            if (data.Length < _settings.DataPackLength)
                data = data.PadLeft(_settings.DataPackLength, _settings.Filler);
            if (data.Length > _settings.DataPackLength)
                data = data.Remove(_settings.DataPackLength);
            return data;
        }

        private string GetCommandTypeString(int commandEnumValue)
        {
            return commandEnumValue.ToString().PadLeft(_settings.CommandLength, '0');
        }
    }
}