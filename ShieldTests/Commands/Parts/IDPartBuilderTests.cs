using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Commands.Parts.CommandPartValidators;
using Shield.Commands.Parts;

namespace Shield.Messaging.Commands.Parts.Tests
{
    public class IDPartBuilderTests
    {

        [Fact()]
        public void GetBuilderTest()
        {
            IPartBuilder builder = new IDPartBuilder(new AllwaysGoodValidator());

            Func<string, IPart> actual = builder.GetBuilder();

            var test = actual.Invoke("AA");
            IPart b = new IDPart("AA", new AllwaysGoodValidator());

            Assert.True(test.Equals(b));
        }
    }
}