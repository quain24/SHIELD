using Shield.CommandPartValidators.RawData;

namespace Shield.Messaging.RawData
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