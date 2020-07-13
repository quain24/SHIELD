using System.Collections.Generic;
using System.Linq;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public class PartValidatorBuilder : IPartValidatorBuilder
    {
        private enum ValidatorType
        {
            MinimumLength,
            MaximumLength,
            AllowOnlyAlphanumeric,
            ForbidChars,
            AllwaysGood,
            AllwaysBad
        }

        private readonly Dictionary<ValidatorType, IPartValidator> _validators = new Dictionary<ValidatorType, IPartValidator>();

        public IPartValidatorBuilder MinimumLength(int length)
        {
            _validators[ValidatorType.MinimumLength] = new MinLengthValidator(length);
            return this;
        }

        public IPartValidatorBuilder MaximumLength(int length)
        {
            _validators[ValidatorType.MaximumLength] = new MaxLengthValidator(length);
            return this;
        }

        public IPartValidatorBuilder ForbidChars(params char[] forbiddenCharacters)
        {
            _validators[ValidatorType.ForbidChars] =
                _validators.TryGetValue(ValidatorType.ForbidChars, out var alreadyForbiddenChars)
                ? new CompositValidator(alreadyForbiddenChars, new ForbiddenCharsValidator(forbiddenCharacters)) as IPartValidator
                : new ForbiddenCharsValidator(forbiddenCharacters);

            return this;
        }

        public IPartValidatorBuilder AllowOnlyAlphaNumeric()
        {
            _validators[ValidatorType.AllowOnlyAlphanumeric] = new OnlyAlphanumericAllowedValidator();
            return this;
        }

        public IPartValidatorBuilder AllwaysValidateAsGood()
        {
            _validators.Clear();
            _validators[ValidatorType.AllwaysGood] = AllwaysGoodValidatorSingleton.Instance;
            return this;
        }

        public IPartValidatorBuilder AllwaysValidateAsBad()
        {
            _validators.Clear();
            _validators[ValidatorType.AllwaysBad] = AllwaysBadValidatorSingleton.Instance;
            return this;
        }

        public IPartValidatorBuilder Reset()
        {
            _validators.Clear();
            return this;
        }

        public IPartValidator Build() => new CompositValidator(_validators.Values.ToArray());
    }
}