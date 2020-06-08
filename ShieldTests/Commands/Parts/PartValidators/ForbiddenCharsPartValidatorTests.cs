using Xunit;
using Shield.Messaging.Commands.Parts.PartValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Shield.Messaging.Commands.Parts.PartValidators.Tests
{
    public class ForbiddenCharsPartValidatorTests
    {
        public ForbiddenCharsPartValidatorTests()
        {
            Validator = new ForbiddenCharsValidator(new char[]{'*'});
        }

        ForbiddenCharsValidator Validator;

        [Fact()]
        public void ForbiddenCharsPartValidatorTest()
        {
            string a = "abcde";
            string b = "*abcd";

            var at = Validator.Validate(a);
            var bt = Validator.Validate(b);
            Debug.WriteLine("");
        }

        [Fact()]
        public void ValidateTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}