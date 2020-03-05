using Shield.Enums;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class CommandModelFactory : ICommandModelFactory
    {
        private Func<ICommandModel> _commandFactory;
        private readonly IIdGenerator _idGenerator;
        private int _idLength = 0;

        public CommandModelFactory(Func<ICommandModel> commandFactory, IIdGenerator idGenerator)
        {
            // Autofac factory
            _commandFactory = commandFactory;
            _idGenerator = idGenerator;
        }

        public ICommandModel Create(CommandType type = CommandType.Empty, string idOverride = "", long timestampOverride = 0)
        {
            if (!Enum.IsDefined(typeof(CommandType), type))
            {
                return null;
            }

            ICommandModel output = _commandFactory();

            output.CommandType = type;
            output.Id = string.IsNullOrWhiteSpace(idOverride) ? _idGenerator.GetNewID() : idOverride;
            output.TimeStamp = timestampOverride <= 0 ? Helpers.Timestamp.TimestampNow : timestampOverride;

            return output;
        }
    }
}