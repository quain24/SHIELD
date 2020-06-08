using Shield.Extensions;
using System.Linq;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public class OnlyAlphanumericAllowedValidator : IPartValidator
    {
        public bool Validate(string data) => data.IsAlphanumeric();
    }
}