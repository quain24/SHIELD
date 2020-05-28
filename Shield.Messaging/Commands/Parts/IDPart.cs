using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;
using System;

namespace Shield.Commands.Parts
{
    public sealed class IDPart : Part
    {
        public override string Data => base.Data.ToUpperInvariant();

        internal IDPart(string data, IPartValidator validator)
            : base(data, validator)
        {
        }
    }
}