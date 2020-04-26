using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace ShieldTests.HardwareCom.CommandProcessing.TestData
{
    internal class TestErrorCommandsClassData : TheoryData<ICommandModel, string>
    {
        private readonly DefaultConfigForCommandCreation _settings = new DefaultConfigForCommandCreation();
        private readonly CommandModelFactory _commandFactory = new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel()));

        public TestErrorCommandsClassData() => GenerateCommands();

        private void GenerateCommands()
        {
            List<string> commandStrings = new List<string>()
            {
                "*0015*0001*123",
                "asdaggsgdfg",
                "953945783957932587566776259684590873568",
                "1",
                "*0000*0000*00000000000000000000000000",
                "*...*",
                "*0018*1234*123456789012",
                "***********************"
            };

            ReplaceSpecialSymbolsWithOnesFromSetup(commandStrings);

            foreach (var s in commandStrings)
                AddToTestCase(s);
        }

        private void ReplaceSpecialSymbolsWithOnesFromSetup(List<string> source)
        {
            source.ForEach(s => s = s.Replace('*', _settings.Separator).Replace('.', _settings.Filler));
        }

        private void AddToTestCase(string commandString) =>
            Add(_commandFactory.Create(CommandType.Error, GetId(), long.Parse("0"), GetDataPack(commandString)), commandString);

        private string GetId() => _settings.Filler.ToString().PadLeft(_settings.IDLength, _settings.Filler);

        private string GetDataPack(string data) => data.Length > _settings.DataPackLength ? data.Substring(0, _settings.DataPackLength) : data;
    }
}