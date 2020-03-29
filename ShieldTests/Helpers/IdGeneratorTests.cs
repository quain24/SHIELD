using Shield.Helpers;
using System;
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
            string result = new IdGenerator(length).GetNewID();
            Assert.Equal(length, result.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-512)]
        public void IdGenerator_ThrowsExceptionWhenLengthIsOutOfBounds(int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IdGenerator(length));
        }
    }
}