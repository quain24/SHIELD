using System;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public sealed class AllwaysBadValidatorSingleton : IPartValidator
    {
        private static readonly Lazy<AllwaysBadValidatorSingleton> AllwaysBadValidator = new Lazy<AllwaysBadValidatorSingleton>(() => new AllwaysBadValidatorSingleton());

        private AllwaysBadValidatorSingleton()
        {
        }

        public static AllwaysBadValidatorSingleton Instance => AllwaysBadValidator.Value;

        public bool Validate(string data) => false;
    }
}