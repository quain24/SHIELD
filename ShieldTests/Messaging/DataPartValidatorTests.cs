using Shield.Messaging.Commands.Parts.CommandPartValidators;
using Xunit;

namespace ShieldTests.Messaging
{
    public class DataPartValidatorTests
    {
        [Fact]
        public void Will_return_true_given_proper_data_pack()
        {
            var validator = new DataPartValidator(5, '!');
            var testValue = "1@#99";

            var actual = validator.Validate(testValue);

            Assert.True(actual);
        }
    }
}