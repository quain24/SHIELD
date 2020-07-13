using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;

namespace Shield.Messaging.DeviceHandler
{
    public class CommandTranslator
    {
        private readonly CommandFactory _commandFactory;
        private readonly RawCommandFactory _rawCommandFactory;

        public CommandTranslator(CommandFactory commandFactory, RawCommandFactory rawCommandFactory)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _rawCommandFactory = rawCommandFactory ?? throw new ArgumentNullException(nameof(rawCommandFactory));
        }

        public ICommand TranslateFrom(RawCommand rawCommand) => _commandFactory.TranslateFrom(rawCommand);

        public RawCommand TranslateFrom(ICommand command) => _rawCommandFactory.TranslateFrom(command);
    }
}