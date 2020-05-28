using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;

namespace Shield.Commands.Parts
{
    public sealed class HosIDPart : Part
    {
        public override string Data => base.Data.ToUpperInvariant();

        internal HosIDPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}