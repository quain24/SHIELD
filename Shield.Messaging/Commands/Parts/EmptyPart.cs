using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;

namespace Shield.Commands.Parts
{
    public class EmptyPart : Part
    {
        internal EmptyPart(IPartValidator validator) : base(string.Empty, validator)
        {
        }
    }
}
