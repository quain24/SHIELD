using Shield.Messaging.Commands.Parts.PartValidators;

namespace Shield.Messaging.Commands.Parts
{
    internal class TargetPart : Part
    {
        public TargetPart(string data, IPartValidator validator) : base(data, validator)
        {
        }
    }
}