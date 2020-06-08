using System;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public sealed class AllwaysBadValidatorSingleton : IPartValidator
    {
        private static readonly Lazy<AllwaysBadValidatorSingleton> _allwaysBadValidator = new Lazy<AllwaysBadValidatorSingleton>(() => new AllwaysBadValidatorSingleton());

        private AllwaysBadValidatorSingleton()
        {
        }

        public static AllwaysBadValidatorSingleton Instance => _allwaysBadValidator.Value;

        public bool Validate(string data) => false;
    }
}