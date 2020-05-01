using Shield.HardwareCom.RawDataProcessing;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace ShieldTests.HardwareCom.RawDataProcessing
{
    public class IncomingDataPreparerTests
    {
        public IncomingDataPreparer TestPreparer;

        public IncomingDataPreparerTests()
        {
            TestPreparer = new IncomingDataPreparer(4, 4, 10,
                                                     new Regex("[*][0-9]{4}[*][a-zA-Z0-9]{4}[*]"),
                                                     '*');
        }

        [Theory]
        [InlineData("*0001*0000")]
        [InlineData("*0001*000")]
        [InlineData("*0001*")]
        [InlineData("*00")]
        public void DataSearch_will_returns_empty_list_when_given_too_short_data(string data)
        {
            var output = TestPreparer.DataSearch(data);
            Assert.Equal(new List<string>(), output);
        }

        [Theory]
        [InlineData("*0018*0001*0123456789")]
        [InlineData("*0018*0001*0123456789", "*0018*0002*1231231231")]
        [InlineData("*0018*0001*0123456789", "*0018*0002*1231231231", "*0018*0003*1231231231")]
        [InlineData("*0001*0001*", "*0018*0002*1231231231")]
        public void DataSearh_will_return_list_of_commands_given_good_data(params string[] data)
        {
            var actual = new List<string>();
            var expected = new List<string>();

            foreach (var s in data)
            {
                actual.AddRange(TestPreparer.DataSearch(s));
                expected.Add(s);
            }

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("*0018*0001*012345678901234567890123456789", "*0018*0001*0123456789", "01234567890123456789")]
        [InlineData("*0018*0001*0123456789*0018*0002*012345678901234567890123456789", "*0018*0001*0123456789", "*0018*0002*0123456789", "01234567890123456789")]
        [InlineData("123456789*0018*0001*0123456789*0018*0002*0123456789", "123456789", "*0018*0001*0123456789", "*0018*0002*0123456789")]
        [InlineData("*0018*0001*0123456789123456789*0018*0002*0123456789", "*0018*0001*0123456789", "123456789", "*0018*0002*0123456789")]
        public void DataSearh_should_return_good_command_and_trash_given_good_data_and_bad_data_with_no_separator_in_bad_data(string data, params string[] expectedData)
        {
            var actual = TestPreparer.DataSearch(data);
            var expected = new List<string>();

            expected.AddRange(expectedData);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("0000000000000")]
        [InlineData("123123123123")]
        public void DataSearch_should_return_bad_data_if_given_bad_data_longer_than_command_length_with_no_separator(string data)
        {
            var actual = TestPreparer.DataSearch(data);
            var expected = new List<string>() { data };

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("*0018*0001*01234567",
                    "*0018*0002*0123456789")]
        [InlineData("*0018*0001*01234567",
                    "*0018*0002*0123456789",
                    "*0018*0001*01234567",
                    "*0018*0002*0123456789")]
        [InlineData("*0018*0001*01234567",
                    "*0018*0001*01234567",
                    "*0018*0001*01234567",
                    "*0018*0002*0123456789",
                    "*0018*0001*01234567",
                    "*0018*0002*0123456789")]
        [InlineData("*0018*0001*01234567",
                    "*0018*0001*",
                    "*0018*0001*01234567",
                    "*0018*0002*0123456789",
                    "*0018*0001*01234567",
                    "*********",
                    "*0003*0001*",
                    "*0018*0002*0123456789",
                    "*0001*0001*")]
        public void DataSearch_returns_junk_and_data_given_too_short_data_command_and_normal_data_command_no_junk_at_end(params string[] expectedData)
        {
            var data = string.Concat(expectedData);
            var actual = TestPreparer.DataSearch(data);
            var expected = new List<string>();

            expected.AddRange(expectedData);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Returns_empty_list_if_passed_empty_data_string()
        {
            var actual = TestPreparer.DataSearch(string.Empty);

            Assert.Empty(actual);
        }

        [Fact]
        public void Returns_empty_list_if_passed_null()
        {
            var actual = TestPreparer.DataSearch(null);

            Assert.Empty(actual);
        }

        [Theory]
        [InlineData("*0018*0001*", "0123456789")]
        [InlineData("*0018*0001*", "012345", "6789")]
        [InlineData("*0018", "*0001*", "012345", "6789")]
        [InlineData("*0001", "*", "00", "01*")]
        public void Will_compose_whole_command_given_two_or_more_proper_partials_in_order(params string[] expectedData)
        {
            var expected = string.Concat(expectedData);
            var actual = new List<string>();

            foreach (var s in expectedData)
                actual.AddRange(TestPreparer.DataSearch(s));

            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [InlineData("***aasdjhasfkk*skhf*sdkfhskhfkshf90489235h5*", "***aasdjhasfkk*skhf*sdkfhskhfkshf90489235h5")] // Last separator in buffer in case of future data
        [InlineData("sdofghjdfilghjsdfgh", "sdofghjdfilghjsdfgh")]
        [InlineData("*00AB*0001*", "*00AB*0001")]    // Last separator in buffer in case of future data
        public void Returns_junk_given_completly_bad_data(string data, params string[] expectedData)
        {
            var actual = TestPreparer.DataSearch(data);
            var expected = expectedData.ToList();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("***gsdfgg*",
                    "**0001*0001**000A*0001**0018*0001*adkkk",
                    "dkajsd*0002*0002*a___*0018*0003*0123456789*0003*AAAA****",
                    "***gsdfgg**", "*0001*0001*", "*000A*0001*", "*0018*0001*adkkkdkajs", "d", "*0002*0002*", "a___", "*0018*0003*0123456789", "*0003*AAAA*")]
        public void Returns_good_and_bad_commands_given_good_and_bad_commands_or_partials(params string[] expectedData)
        {
            var split = new List<string>() { expectedData[0], expectedData[1], expectedData[2] };
            List<string> actual = new List<string>();
            foreach (var s in split)
                actual.AddRange(TestPreparer.DataSearch(s));

            List<string> expected = new List<string>();
            for (int i = 3; i < expectedData.Length; i++)
                expected.Add(expectedData[i]);

            Assert.Equal(expected, actual);
        }
    }
}