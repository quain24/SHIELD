using Shield.Messaging.Commands.Parts.PartValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Commands.Parts
{
    class TargetPart : Part
    {
        public TargetPart(string data, IPartValidator validator) : base(data, validator)
        {
        }
    }
}
