using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;

namespace Shield.Commands.Parts
{
    public sealed class DataPart : Part
    {
        public DataPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}