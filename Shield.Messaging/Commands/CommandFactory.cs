using Shield.Extensions;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.Commands
{
    public class CommandFactory
    {
        private readonly char _separator;
        private readonly IPartFactory _factory;
        private readonly CommandFactoryAutoFacAdapter _commandFactory;

        private readonly List<Enums.Command.PartType> _requiredParts = new List<Enums.Command.PartType>()
        {
            Enums.Command.PartType.HostID,
            Enums.Command.PartType.ID,
            Enums.Command.PartType.Type,
            Enums.Command.PartType.Data
        };

        public CommandFactory(char separator, IPartFactory factory, CommandFactoryAutoFacAdapter commandFactory)
        {
            _separator = separator;
            _factory = factory;
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        }

        public ICommand TranslateFrom(RawCommand rawCommand)
        {
            var splittedData = rawCommand?.ToString().SplitBy(_separator).ToList() ?? throw new ArgumentNullException(nameof(rawCommand));

            // TODO think about moving splitting into raw command itself on construction - this would require a rawcommand factory with knowledge of separator char

            var partsEnumerator = _requiredParts.GetEnumerator();
            var dataEnumerator = splittedData.GetEnumerator();

            var parts = new List<IPart>();

            while (partsEnumerator.MoveNext())
            {
                parts.Add(dataEnumerator.MoveNext()
                    ? _factory.GetPart(partsEnumerator.Current, dataEnumerator.Current)
                    : _factory.GetPart(Enums.Command.PartType.Empty, string.Empty));
            }

            return _commandFactory.GetCommand(parts[0], parts[1], parts[2], parts[3]);
        }
    }
}