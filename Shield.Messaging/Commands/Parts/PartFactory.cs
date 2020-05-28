using System;
using System.Collections.Generic;
using static Shield.Command;

namespace Shield.Messaging.Commands.Parts
{
    public class PartFactory
    {
        private readonly IReadOnlyDictionary<PartType, PartFactoryAutofacAdapter> _factories;

        public PartFactory(IReadOnlyDictionary<PartType, PartFactoryAutofacAdapter> factories)
        {
            _factories = factories ?? throw new ArgumentNullException(nameof(factories), "Cannot initialize PartFactory with a NULL");
        }

        public IPart GetPart(PartType type, string data)
        {
            var factory = _factories.TryGetValue(type, out var result)
                ? result
                : throw new ArgumentOutOfRangeException(nameof(type), "There is no internal factory for requested type or requested type is outside of enum bounds.");
            return factory.GetPart(data);
        }
    }
}