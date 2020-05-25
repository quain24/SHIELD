using System;
using System.Text.RegularExpressions;

namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public class DataPartValidator : IPartValidator
    {
        private readonly int _length;
        private readonly Regex _pattern;

        public DataPartValidator(int length, params char[] forbiddenCharacters)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), $"{nameof(length)} cannot be less then 1.");
            _length = length;
            _pattern = GeneratePattern(forbiddenCharacters);
        }

        private Regex GeneratePattern(params char[] forbiddenCharacters)
        {
            var except = RegexEscapeChars.AddEscapeCharsTo(new string(forbiddenCharacters));

            except = forbiddenCharacters.Length > 0
                ? $"[^{except}]{{{_length}}}" // Any string of given length without forbidden chars
                : $".{{{_length}}}";          // Any string of given length

            return new Regex(except);
        }

        public bool Validate(string data) => data.Length == _length && _pattern.IsMatch(data);
    }
}