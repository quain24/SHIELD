using Shield.HardwareCom.RawDataProcessing;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace ShieldTests.HardwareCom.RawDataProcessing
{
    public class IncomingDataPreparerTests
    {
        public IncomingDataPreparer2 TestPreparer;
        public string goodData => "*0001*0001*0123456789";
        public string badDataPack = "*0002*0002*012345*789";
        public string badDataPackShort = "*0003*0003*012345*7";
        public string badDataShort = "*0004*0004*01234567";
        public string good = "*0005*0005*";
        public string badId = "*0006*.$06*";
        public string badShort = "*0007*12";
        public string badMissingFrontSep = "0008*0008*";

        public IncomingDataPreparerTests()
        {
            TestPreparer = new IncomingDataPreparer2(4, 4, 10,
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
    }
}