using System;
using System.Text.RegularExpressions;

namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public class AllAlphanumericAllowedValidator : IPartValidator
    {
        private readonly int _length;
        private readonly Regex _pattern;

        public AllAlphanumericAllowedValidator(int length)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), $"{nameof(length)} cannot be less then 1.");
            _length = length;
            _pattern = CreatePattern(_length);
        }

        private Regex CreatePattern(int length) => new Regex($"[a-zA-Z0-9]{{{length}}}");

        public bool Validate(string data) => data.Length == _length && _pattern.IsMatch(data);
    }
}