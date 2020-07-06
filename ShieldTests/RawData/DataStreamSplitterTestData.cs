using System.Collections.Generic;
using Xunit;

namespace ShieldTests.RawData
{
    public class DataStreamSplitterTestData : TheoryData<List<string>, List<string>>
    {
        public DataStreamSplitterTestData()
        {
            PrepareData();
        }

        private void PrepareData()
        {
            var a = new List<string>() { "*12345*", "12345", "*", "1234567890*" };
            var b = new List<string>() { "12345", "12345", "1234567890" };
            Add(a, b);

            a = new List<string>() { "******123*\\*12*12345", "67890****", "12345", "*", "12345" };
            b = new List<string>() { "1234567890", "12345"};
            Add(a, b);

            a = new List<string>() { "*", "67890****", "12345"};
            b = new List<string>() { "67890" };
            Add(a, b);

            a = new List<string>() { "*", "*", "12345", "*" };
            b = new List<string>() { "12345" };
            Add(a, b);
        }
    }
}