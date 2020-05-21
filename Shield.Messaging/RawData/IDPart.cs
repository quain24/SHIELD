using Shield.CommandPartValidators.RawData;

namespace Shield.Messaging.RawData
{
    public sealed class IDPart : Part
    {
        public override string Data => base.Data.ToUpperInvariant();

        public IDPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}