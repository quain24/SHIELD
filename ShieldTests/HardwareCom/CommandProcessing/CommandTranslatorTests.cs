using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using ShieldTests.HardwareCom.CommandProcessing.TestData;
using System;
using Xunit;

namespace ShieldTests.HardwareCom.CommandProcessing
{
    public class CommandTranslatorTests
    {
        private static readonly DefaultConfigForCommandCreation _defaults =
            new DefaultConfigForCommandCreation();

        private static readonly CommandTranslatorSettings _settings =
            new CommandTranslatorSettings(
                _defaults.Separator, _defaults.Filler, _defaults.CommandLength,
                _defaults.IDLength, _defaults.DataPackLength, _defaults.HostIDLength);

        private readonly ICommandModelFactory _factory =
            new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel()));

        public CommandTranslatorTests() => CommandTranslator = new CommandTranslator(_settings, _factory);

        private readonly CommandTranslator CommandTranslator;

        private string Message(string paramName, string expected, string actual)
        {
            return "\n" + paramName + ":\nexpected: " + expected + "\nactual: " + actual + "\n";
        }

        [Theory]
        [ClassData(typeof(TestCommandsClassData))]
        public void FromString_should_returns_proper_command_given_proper_string(ICommandModel expected, string commandStrings)
        {
            var actual = CommandTranslator.FromString(commandStrings);

            Assert.True(expected.Id == actual.Id, Message("ID", expected.Id, actual.Id));
            Assert.True(expected.CommandType == actual.CommandType, Message("CommandType", expected.Id, actual.Id));
            Assert.True(expected.Data == actual.Data, Message("Data", expected.Id, actual.Id));
        }

        [Theory]
        [ClassData(typeof(TestErrorCommandsClassData))]
        public void FromString_should_return_error_type_command_filled_with_recovered_data_given_bad_string(ICommandModel expected, string commandString)
        {
            var actual = CommandTranslator.FromString(commandString);

            Assert.True(expected.Id == actual.Id, Message("ID", expected.Id, actual.Id));
            Assert.True(expected.CommandType == actual.CommandType, Message("CommandType", expected.Id, actual.Id));
            Assert.True(expected.Data == actual.Data, Message("Data", expected.Id, actual.Id));
        }

        [Theory]
        [ClassData(typeof(TestUnknownCommandClassData))]
        public void FromString_should_return_unknown_command_without_data_given_out_of_command_enum_type_with_or_without_data(ICommandModel expected, string commandString)
        {
            var actual = CommandTranslator.FromString(commandString);

            Assert.True(expected.Id == actual.Id, Message("ID", expected.Id, actual.Id));
            Assert.True(expected.CommandType == actual.CommandType, Message("CommandType", expected.Id, actual.Id));
            Assert.True(expected.Data == actual.Data, Message("Data", expected.Id, actual.Id));
        }

        [Fact]
        public void FromString_should_throw_exception_when_given_null_instead_of_string()
        {
            Assert.Throws<ArgumentNullException>(() => CommandTranslator.FromString(null));
        }

        [Theory]
        [ClassData(typeof(TestCommandsClassData))]
        public void FromCommand_should_return_proper_raw_string_given_proper_ICommandModel(ICommandModel data, string expected)
        {
            var actual = CommandTranslator.FromCommand(data);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FromCommand_should_throw_argument_null_exception_if_given_NULL_instead_of_proper_ICommandModel()
        {
            Assert.Throws<ArgumentNullException>(() => CommandTranslator.FromCommand(null));
        }

        [Fact]
        public void FromCommand_should_return_rmpty_string_given_unknown_command_type()
        {
            ICommandModel command = _factory.Create(CommandType.Empty, "0".PadLeft(_settings.IdLength, '0'));

            // Factory will not allow wrong command type be created, therefore:
            command.CommandType = (CommandType)Enum.GetValues(typeof(CommandType)).Length + 1;

           var actual = CommandTranslator.FromCommand(command);

           Assert.True(string.IsNullOrEmpty(actual));
        }
    }
}