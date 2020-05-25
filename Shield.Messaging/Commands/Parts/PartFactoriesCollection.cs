using Shield.Messaging.Commands.Parts.CommandPartValidators;
using System;
using System.Collections.Generic;
using static Shield.Command;

namespace Shield.Messaging.Commands.Parts
{
    public class PartFactoriesCollection
    {
        private readonly IDictionary<PartType, (Func<string, IPartValidator, IPart>, IPartValidator)> _combinedPartValidators;

        public PartFactoriesCollection(IDictionary<PartType, (Func<string, IPartValidator, IPart>, IPartValidator)> combinedPartValidatorPairs)
        {
            _combinedPartValidators = combinedPartValidatorPairs;
        }

        public (Func<string, IPartValidator, IPart>, IPartValidator) GetFactoryOfType(PartType type)
        {
            if (_combinedPartValidators.TryGetValue(type, out var factoryValidatorPair))
                return factoryValidatorPair;
            else
                throw new ArgumentOutOfRangeException(nameof(type), $"There is no Factory / validator pair for requested type {Enum.GetName(typeof(PartType), type)} in passed collection");
        } // TODO Is value tuple really a good way? Do i need this collection? maybe i should pass this factories directly into main partFactory object
    }
}