using System;
using System.Linq;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public class CompositeValidator : IPartValidator
    {
        private readonly IPartValidator[] _validators;

        public CompositeValidator(params IPartValidator[] validators)
        {
            if (validators is null || validators.Length < 1)
                throw new ArgumentNullException(nameof(validators), "CompositeValidator cannot be composed from nothing");
            _validators = validators;
        }

        public bool Validate(string data) => _validators.All(v => v.Validate(data));
    }
}