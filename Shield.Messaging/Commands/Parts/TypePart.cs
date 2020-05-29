using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;

namespace Shield.Commands.Parts
{
    public sealed class TypePart : Part
    {
        public override string Data => base.Data.ToUpperInvariant();

        internal TypePart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}