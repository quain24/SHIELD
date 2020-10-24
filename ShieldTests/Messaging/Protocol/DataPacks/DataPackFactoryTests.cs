using Shield.Messaging.Protocol.DataPacks;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;

namespace ShieldTests.Messaging.Protocol.DataPacks
{
    public class DataPackFactoryTests
    {
        private readonly ITestOutputHelper _output;
        private readonly IDataPackFactory _factory = new ReflectionBasedDataPackFactory();

        public DataPackFactoryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Given_test_multiple_strings_will_return_StringDataPack()
        {
            var output = _factory.CreateFrom("test", "ssss");
            Assert.IsType<StringArrayDataPack>(output);
            Assert.Equal($"test{Shield.GlobalConfig.DataPackSettings.DataPackSeparator}ssss", output.GetDataInTransmittableFormat());
        }

        [Fact]
        public void Given_test_string_will_return_StringDataPack()
        {
            var output = _factory.CreateFrom("data");
            Assert.IsType<StringDataPack>(output);
        }

        [Fact]
        public void Given_empty_strings_should_return_EmptyDataPack_instance()
        {
            var output = _factory.CreateFrom("", "", "");
            Assert.IsType<EmptyDataPackSingleton>(output);
        }

        [Fact]
        public void Given_empty_string_should_return_EmptyDataPack_instance()
        {
            var data = "";
            var output = _factory.CreateFrom(data);

            Assert.IsType<EmptyDataPackSingleton>(output);
        }

        [Fact]
        public void Given_double_should_return_DoubleDataPack()
        {
            var data = 10d;
            var output = _factory.CreateFrom(data);

            Assert.IsType<DoubleDataPack>(output);
            Assert.Equal(data.ToString(CultureInfo.InvariantCulture), output.GetDataInTransmittableFormat());
        }

        [Fact]
        public void Given_int_should_return_Int32DataPack()
        {
            var data = 10;
            var output = _factory.CreateFrom(data);

            Assert.IsType<Int32DataPack>(output);
            Assert.Equal(data.ToString(CultureInfo.InvariantCulture), output.GetDataInTransmittableFormat());
        }

        [Fact]
        public void Given_NULL_should_return_EmptyDataPack_instance()
        {
            object data = null;
            var output = _factory.CreateFrom(data);

            Assert.IsType<EmptyDataPackSingleton>(output);
        }

        [Fact]
        public void Given_object_type_should_return_JsonDataPack()
        {
            var testObject = new { data = "test Data", id = 1 };
            var output = _factory.CreateFrom(testObject);

            Assert.IsType<JsonDataPack>(output);
        }
    }
}