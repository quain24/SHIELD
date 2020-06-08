using System;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    internal class MinLengthValidator : IPartValidator
    {
        private readonly int _minLength;

        public MinLengthValidator(int length)
        {
            _minLength = length >= 0 ? length : throw new ArgumentOutOfRangeException(nameof(length), "Minimum length cannot be negative");
        }

        public bool Validate(string data) => data.Length >= _minLength;
    }
}