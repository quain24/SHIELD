using Shield.CommandPartValidators.RawData;
using Shield.Messaging.RawData;
using Shield.Messaging.RawData.CommandPartValidators;
using Xunit;

namespace ShieldTests.Messaging
{
    public class IDPartTests
    {
        public IPartValidator validator = new AllAlphanumericAllowedValidator(4);

        [Fact]
        public void Is_valid_given_proper_ID()
        {
            IPart idPart = new IDPart("AbCd", validator);

            var actual = idPart.IsValid;
            Assert.True(actual);
        }

        [Fact]
        public void Two_same_valued_instances_will_be_equal()
        {
            IPart idPartOne = new IDPart("AbCd", validator);
            IPart idPartTwo = new IDPart("AbCd", validator);

            Assert.True(idPartOne.Equals(idPartTwo));
        }
    }
}