using System;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public class TypePartValidator : IPartValidator
    {
        private readonly KnownCommandTypes _knownTypes;

        public TypePartValidator(KnownCommandTypes knownTypes)
        {
            _knownTypes = knownTypes ?? throw new ArgumentNullException(nameof(knownTypes));
        }

        public bool Validate(string rawCommandType) => _knownTypes.Contains(rawCommandType);
    }
}