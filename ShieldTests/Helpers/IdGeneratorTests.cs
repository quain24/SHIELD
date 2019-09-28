using Xunit;

namespace ShieldTests.Helpers
{
    public class IdGeneratorTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(1)]
        public void IdGenerator_ReturnsRandomStringWhenGivenCorrectLength(int length)
        {
            string result = Shield.Helpers.IdGenerator.GetId(length);
            Assert.NotNull(result);
            Assert.Equal(length, result.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-512)]
        public void IdGenerator_ReturnsNullGuivenWrongData(int length)
        {
            string result = Shield.Helpers.IdGenerator.GetId(length);

            Assert.Null(result);
        }
    }
}