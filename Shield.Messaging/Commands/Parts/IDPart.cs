﻿using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;
using System;

namespace Shield.Commands.Parts
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