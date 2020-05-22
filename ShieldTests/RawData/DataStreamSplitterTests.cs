using Xunit;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShieldTests.RawData;

namespace Shield.Messaging.RawData.Tests
{
    public class DataStreamSplitterTests
    {

        public DataStreamSplitterTests()
        {
            Splitter = new DataStreamSplitter('*', 5, 10);
        }

        public DataStreamSplitter Splitter {  get; }

        [Theory]
        [InlineData("*12345*1234567890*", "12345", "1234567890")]
        public void DataStreamSplitterTest(string data, params string[] expected)
        {
            var actual = Splitter.Split(data);
            var expectedData = expected.ToList();

            Assert.Equal(expectedData, actual);
        }

        [Theory]
        [InlineData("*12345*","1234567890*", "12345", "1234567890")]
        public void Given_two_separate_inputs_will_give_two_outputs(params string[] data)
        {
            List<string> actual = Splitter.Split(data[0]).ToList();
            actual.AddRange(Splitter.Split(data[1]));
            var expectedData = new List<string>() { data[2], data[3]};

            Assert.Equal(expectedData, actual);
        }

        // TODO More test to be sure, need to fix those already written so the wont look like crap.
        [Theory]
        [InlineData("*12345***", "12345", "67890*", "12345", "1234567890")]
        [InlineData("*12345**12*", "*12345", "67890*", "12345", "1234567890")]
        public void Given_three_separate_inputs_with_one_error_will_give_two_outputs(params string[] data)
        {
            List<string> actual = Splitter.Split(data[0]).ToList();
            actual.AddRange(Splitter.Split(data[1]));
            actual.AddRange(Splitter.Split(data[2]));
            var expectedData = new List<string>() { data[3], data[4] };

            Assert.Equal(expectedData, actual);
        }

        [Theory]
        [ClassData(typeof(DataStreamSplitterTestData))]
        public void Given_splitted_raw_good_and_bad_data_should_return_raw_commands(List<string> data, List<string> expected)
        {
            var actual = new List<string>();
            data.ForEach(d => actual.AddRange(Splitter.Split(d)));

            Assert.Equal(actual, expected);
        }
    }
}