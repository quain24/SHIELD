using Shield.CommandPartValidators.RawData;

namespace Shield.Messaging.RawData
{
    public sealed class TypePart : Part
    {
        public override string Data => base.Data.ToUpperInvariant();

        public TypePart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}