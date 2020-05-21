using Shield.CommandPartValidators.RawData;
using Shield.Messaging.RawData.CommandPartValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.RawData
{
    public class EmptyPart : Part
    {
        public EmptyPart(IPartValidator validator) : base(string.Empty, validator)
        {
        }
    }
}
