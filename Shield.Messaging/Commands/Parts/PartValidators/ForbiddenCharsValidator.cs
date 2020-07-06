using Shield.Extensions;
using System;
using System.Linq;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public class ForbiddenCharsValidator : IPartValidator
    {
        private const int IndexNotFound = -1;
        private readonly char[] _characters;
        private Func<string, bool> _executeValidation;

        public ForbiddenCharsValidator(params char[] forbiddenCharacters)
        {
            _characters = forbiddenCharacters?.Distinct().ToArray() ?? Array.Empty<char>();
            DetermineValidationMethod(_characters);
        }

        private void DetermineValidationMethod(char[] forbiddenCharacters)
        {
            _executeValidation = forbiddenCharacters.IsNullOrEmpty()
                ? new Func<string, bool> ((_) => true)
                : new Func<string, bool> ((data) => data.IndexOfAny(forbiddenCharacters) == IndexNotFound);
        }

        public bool Validate(string data) => _executeValidation(data);
    }
}