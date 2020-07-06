using Shield.Messaging.Commands.Parts.PartValidators;

namespace Shield.Messaging.Commands.Parts
{
    public class EmptyPart : Part
    {
        internal EmptyPart(IPartValidator validator) : base(string.Empty, validator)
        {
        }
    }
}
