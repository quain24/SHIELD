using System;
using System.Linq;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public class CompositValidator : IPartValidator
    {
        private readonly IPartValidator[] _validators;

        public CompositValidator(params IPartValidator[] validators)
        {
            if (validators is null || validators.Length < 1)
                throw new ArgumentNullException(nameof(validators), "CombinedValidator cannot be composed from nothing");
            _validators = validators;
        }

        public bool Validate(string data) => !_validators.Any(v => !v.Validate(data));
    }
}