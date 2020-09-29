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
            AlwaysGood,
            AlwaysBad
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
                ? new CompositeValidator(alreadyForbiddenChars, new ForbiddenCharsValidator(forbiddenCharacters)) as IPartValidator
                : new ForbiddenCharsValidator(forbiddenCharacters);

            return this;
        }

        public IPartValidatorBuilder AllowOnlyAlphaNumeric()
        {
            _validators[ValidatorType.AllowOnlyAlphanumeric] = new OnlyAlphanumericAllowedValidator();
            return this;
        }

        public IPartValidatorBuilder AlwaysValidateAsGood()
        {
            _validators.Clear();
            _validators[ValidatorType.AlwaysGood] = AllwaysGoodValidatorSingleton.Instance;
            return this;
        }

        public IPartValidatorBuilder AlwaysValidateAsBad()
        {
            _validators.Clear();
            _validators[ValidatorType.AlwaysBad] = AllwaysBadValidatorSingleton.Instance;
            return this;
        }

        public IPartValidatorBuilder Reset()
        {
            _validators.Clear();
            return this;
        }

        public IPartValidator Build() => new CompositeValidator(_validators.Values.ToArray());
    }
}