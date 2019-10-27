using Shield.Enums;
using Shield.HardwareCom.Models;
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

        public ICommandModel Create(CommandType type)
        {
            if (!Enum.IsDefined(typeof(CommandType), type))
            {
                return null;
            }

            ICommandModel output = _commandFactory();

            output.CommandType = type;
            return output;
        }
    }
}