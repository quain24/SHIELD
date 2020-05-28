using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;

namespace Shield.Commands.Parts
{
    public sealed class DataPart : Part
    {
        internal DataPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}