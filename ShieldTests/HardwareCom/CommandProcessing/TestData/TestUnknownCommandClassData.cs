using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Linq;
using Xunit;

namespace ShieldTests.HardwareCom.CommandProcessing.TestData
{
    public class TestUnknownCommandClassData : TheoryData<ICommandModel, string>
    {
        private readonly DefaultConfigForCommandCreation _settings = new DefaultConfigForCommandCreation();
        private readonly CommandModelFactory _commandFactory = new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel()));
        private readonly string _dataPack = "abcdefghijklmnopqrstuvwxyz1234567890";

        public TestUnknownCommandClassData() => GenerateCommands();

        private int[] CommandTypes => (int[])Enum.GetValues(typeof(CommandType));

        private void GenerateCommands()
        {
            GenerateCommand(UnknownID(), false);
            GenerateCommand(UnknownID(), true);
        }

        private int UnknownID() => CommandTypes.Last() + 2;

        private void GenerateCommand(int commandEnumValue, bool addDataPack)
        {
            string data = "";
            string hostId = _settings.HostId.ToUpperInvariant();
            string commandType = GetCommandTypeString(commandEnumValue);
            string id = commandType; // The same value for simplification

            var command = _commandFactory.Create(CommandType.Unknown, id, 0, "", hostId);
            var commandString = _settings.Separator + hostId + _settings.Separator + commandType + _settings.Separator + id + _settings.Separator;

            if (addDataPack)
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