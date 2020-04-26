using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using ShieldTests.HardwareCom.CommandProcessing.TestData;
using System;
using Xunit;

namespace ShieldTests.HardwareCom.CommandProcessing
{
    public class CommandTranslatorTests
    {
        private readonly DefaultConfigForCommandCreation _defaults = new DefaultConfigForCommandCreation();
        private readonly CommandTranslatorSettings _settings;
        private readonly ICommandModelFactory _factory = new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel()));

        public CommandTranslatorTests()
        {
            _settings = new CommandTranslatorSettings(_defaults.Separator,
                                                      _defaults.Filler,
                                                      _defaults.CommandLength,
                                                      _defaults.IDLength,
                                                      _defaults.DataPackLength,
                                                      _defaults.HostIDLength);

            CommandTranslator = new CommandTranslator2(_settings, _factory);
        }

        private readonly CommandTranslator2 CommandTranslator;

        [Theory]
        [ClassData(typeof(TestCommandsClassData))]
        public void Returns_proper_command_given_proper_string(ICommandModel expected, string commandStrings)
        {
            var actual = CommandTranslator.FromString(commandStrings);

            Assert.True(expected.Id == actual.Id, $"\nID:\nexpected: {expected.Id}\nactual: {actual.Id}\n");
            Assert.True(expected.CommandType == actual.CommandType, $"\nCommandType:\nexpected: {expected.CommandType}\nactual: {actual.CommandType}\n");
            Assert.True(expected.Data == actual.Data, $"\nData:\nexpected: {expected.Data}\nactual: {actual.Data}\n");
        }

        [Theory]
        [ClassData(typeof(TestErrorCommandsClassData))]
        public void Returns_error_type_command_filled_with_recovered_data_given_bad_string(ICommandModel expected, string commandString)
        {
            var actual = CommandTranslator.FromString(commandString);

            Assert.True(expected.Id == actual.Id, $"\nID:\nexpected: {expected.Id}\nactual: {actual.Id}\n");
            Assert.True(expected.CommandType == actual.CommandType, $"\nCommandType:\nexpected: {expected.CommandType}\nactual: {actual.CommandType}\n");
            Assert.True(expected.Data ==  actual.Data, $"\nData:\nexpected: {expected.Data}\nactual: {actual.Data}\n");
        }

        [Theory]
        [ClassData(typeof(TestUnknownCommandClassData))]
        public void Returns_unknown_command_without_data_given_out_of_command_enum_type_with_or_without_data(ICommandModel expected, string commandString)
        {
            var actual = CommandTranslator.FromString(commandString);

            Assert.True(expected.Id == actual.Id, $"\nID:\nexpected: {expected.Id}\nactual: {actual.Id}\n");
            Assert.True(expected.CommandType == actual.CommandType, $"\nCommandType:\nexpected: {expected.CommandType}\nactual: {actual.CommandType}\n");
            Assert.True(expected.Data == actual.Data, $"\nData:\nexpected: {expected.Data}\nactual: {actual.Data}\n");
        }

        [Fact]
        public void Throws_exception_when_given_null_instead_of_string()
        {
            Assert.Throws<ArgumentNullException>(() => CommandTranslator.FromString(null));
        }
    }
}