using Shield.CommandPartValidators.RawData;

namespace Shield.Messaging.RawData
{
    public sealed class DataPart : Part
    {
        public DataPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}