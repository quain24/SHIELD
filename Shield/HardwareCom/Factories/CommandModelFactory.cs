using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class CommandModelFactory : ICommandModelFactory
    {
        private Func<ICommandModel> _commandFactory;

        public CommandModelFactory(Func<ICommandModel> commandFactory)
        {
            // Autofac factory
            _commandFactory = commandFactory;
        }

        public ICommandModel Create(CommandType type = CommandType.Empty, string idOverride = "", long timestampOverride = 0)
        {
            if (!Enum.IsDefined(typeof(CommandType), type))
                return null;

            ICommandModel output = _commandFactory();

            output.CommandType = type;
            output.Id = idOverride;
            output.TimeStamp = timestampOverride <= 0 ? Timestamp.TimestampNow : timestampOverride;

            return output;
        }
    }
}