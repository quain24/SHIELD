using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;

namespace Shield.Commands.Parts
{
    public sealed class HosIDPart : Part
    {
        public override string Data => base.Data.ToUpperInvariant();

        public HosIDPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}