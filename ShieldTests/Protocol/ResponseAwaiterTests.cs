using Shield.Messaging.Commands;
using Shield.Messaging.Protocol;
using Shield.Timestamps;
using Xunit;
using Xunit.Abstractions;

namespace ShieldTests.Protocol
{
    public class ResponseAwaiterTests
    {
        private readonly ITestOutputHelper _output;
        private Timeout _timeout;
        private ResponseAwaiter _responseAwaiter;

        public ResponseAwaiterTests(ITestOutputHelper output)
        {
            _output = output;
            Setup();
        }
        public ResponseAwaiter ResponseAwaiter { get => _responseAwaiter; }

        public void Setup()
        {
            _timeout = new Timeout(2);
            _responseAwaiter = new ResponseAwaiter(_timeout);
        }


        [Fact()]
        public void Returns_child_awaiter_when_await_response_called_for_not_already_responded_order()
        {
            var testOrder = new Order("a", "b,", "c", TimestampFactory.Timestamp);
            var output = ResponseAwaiter.AwaitResponse(testOrder);
            
            Assert.IsType(typeof(ChildAwaiter), output);
        }

        [Fact()]
        public void AwaitResponseAsyncTest()
        {
        }

        [Fact()]
        public void AddResponseTest()
        {
        }

        [Fact()]
        public void GetResponseTest()
        {
        }
    }
}