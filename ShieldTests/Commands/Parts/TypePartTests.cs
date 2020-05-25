using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Commands.Parts.CommandPartValidators;
using Shield.Messaging.Commands.Parts;

namespace Shield.Commands.Parts.Tests
{
    public class TypePartTests
    {
        [Fact()]
        public void TypePartTest()
        {
            IPart id1 = new IDPart("AAA", new AllwaysGoodValidator());
            Part id2 = new IDPart("AAA", new AllwaysGoodValidator());
            IPart data1 = new DataPart("AAA", new AllwaysGoodValidator());
            Assert.True(id1.Equals(id2));
        }
    }
}