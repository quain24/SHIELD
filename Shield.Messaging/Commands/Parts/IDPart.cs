using Shield.Messaging.Commands.Parts.PartValidators;

namespace Shield.Messaging.Commands.Parts
{
    public sealed class IDPart : Part
    {
        public override string Data => base.Data.ToUpperInvariant();

        public IDPart(string data, IPartValidator validator) : base(data, validator)
        {
        }
    }
}