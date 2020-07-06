using Shield.Messaging.Commands.Parts.PartValidators;

namespace Shield.Messaging.Commands.Parts
{
    public sealed class DataPart : Part
    {
        internal DataPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}