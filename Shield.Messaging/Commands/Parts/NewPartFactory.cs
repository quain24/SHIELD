using Shield.Messaging.Commands.Parts.CommandPartValidators;
using System;
using System.Collections.Generic;
using static Shield.Command;

namespace Shield.Messaging.Commands.Parts
{
    public class NewPartFactory
    {
        private IDictionary<PartType, Tuple<IPartValidator, Func<string, IPartValidator, IPart>>> _combinedPartValidatorPairs;

        public NewPartFactory(IDictionary<PartType, Tuple<IPartValidator, Func<string, IPartValidator, IPart>>> combinedPartValidatorPairs)
        {
            _combinedPartValidatorPairs = combinedPartValidatorPairs;
        }

        private IPart GetIDPart(string data) => _combinedPartValidatorPairs[PartType.Data].Item2.Invoke(data, _combinedPartValidatorPairs[PartType.ID].Item1);
    }
}

// TODO is this the way to go? Need testing. Can autofac properly inject Auto func factory?
// check timeout factory way from HardwareCom
// Further new ideas in newPartfactory and PartFactory Collection  