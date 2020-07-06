using System;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    internal class MaxLengthValidator : IPartValidator
    {
        private readonly int _length;

        public MaxLengthValidator(int length)
        {
            _length = length >= 0 ? length : throw new ArgumentOutOfRangeException(nameof(length), "Max length cannot be negative");
        }

        public bool Validate(string data) => data.Length <= _length;
    }
}