using Shield.Extensions;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Linq;
using static Shield.Enums.Command;

namespace Shield.Messaging.Commands
{
    public class CommandFactory
    {
        private readonly char _separator;
        private readonly IPartFactory _factory;
        private readonly CommandFactoryAutoFacAdapter _commandFactory;
        private readonly List<PartType> _requiredParts = new List<PartType>()
        {
           PartType.ID,
           PartType.HostID,
           PartType.Target,
           PartType.Order,
           PartType.Data
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

            var partsEnumerator = _requiredParts.GetEnumerator();
            var dataEnumerator = splittedData.GetEnumerator();

            var parts = new Dictionary<PartType, IPart>();

            while (partsEnumerator.MoveNext())
            {
                parts.Add(partsEnumerator.Current, dataEnumerator.MoveNext()
                    ? _factory.GetPart(partsEnumerator.Current, dataEnumerator.Current)
                    : _factory.GetPart(PartType.Empty, string.Empty));
            }

            return _commandFactory.GetCommand(parts[PartType.ID],
                                              parts[PartType.HostID],
                                              parts[PartType.Target],
                                              parts[PartType.Order],
                                              parts[PartType.Data]);
        }
    }
}