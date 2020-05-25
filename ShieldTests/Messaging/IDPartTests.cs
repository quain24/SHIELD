using Shield.Commands.Parts;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;
using Xunit;

namespace ShieldTests.Messaging
{
    public class IDPartTests
    {
        public IPartValidator validator = new AllAlphanumericAllowedValidator(4);

        [Fact]
        public void Is_valid_given_proper_ID()
        {
            Part idPart = new IDPart("AbCd", validator);

            var actual = idPart.IsValid;
            Assert.True(actual);
        }

        [Fact]
        public void Two_same_valued_instances_will_be_equal()
        {
            Part idPartOne = new IDPart("AbCd", validator);
            Part idPartTwo = new IDPart("AbCd", validator);

            Assert.True(idPartOne.Equals(idPartTwo));
        }
    }
}