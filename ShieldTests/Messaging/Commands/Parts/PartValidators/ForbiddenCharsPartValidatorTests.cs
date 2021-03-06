﻿using System.Diagnostics;
using Shield.Messaging.Commands.Parts.PartValidators;
using Xunit;

namespace ShieldTests.Messaging.Commands.Parts.PartValidators
{
    public class ForbiddenCharsPartValidatorTests
    {
        public ForbiddenCharsPartValidatorTests()
        {
            Validator = new ForbiddenCharsValidator(new char[] { '*' });
        }

        private ForbiddenCharsValidator Validator;

        [Fact()]
        public void ForbiddenCharsPartValidatorTest()
        {
            string a = "abcde";
            string b = "*abcd";

            var at = Validator.Validate(a);
            var bt = Validator.Validate(b);
            Debug.WriteLine("");
        }
    }
}