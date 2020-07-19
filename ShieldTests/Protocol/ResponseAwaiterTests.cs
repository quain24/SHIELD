using Shield.Messaging.Commands;
using Shield.Messaging.Commands.States;
using Shield.Messaging.Protocol;
using Shield.Timestamps;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ShieldTests.Protocol
{
    public class ResponseAwaiterTests
    {
        private readonly ITestOutputHelper _output;
        private Timeout _timeout;
        private readonly int _timeoutInMilliseconds = 1000;

        public ResponseAwaiterTests(ITestOutputHelper output)
        {
            _output = output;
            Setup();
        }

        private ResponseAwaiter ResponseAwaiter { get; set; }

        private void Setup()
        {
            _timeout = new Timeout(_timeoutInMilliseconds);
            ResponseAwaiter = new ResponseAwaiter(_timeout);
        }

        [Fact()]
        public void Returns_child_awaiter_when_await_response_called_for_not_already_responded_order()
        {
            var testOrder = new Order("a", "b,", "c", TimestampFactory.Timestamp);
            var output = ResponseAwaiter.GetAwaiter(testOrder);

            Assert.IsType<ChildAwaiter>(output);
        }

        [Fact()]
        public void Returns_AlreadyKnownAwaiter_when_used_to_await_for_already_provided_confirmation()
        {
            var confirmation = new Confirmation("idid", ErrorState.Unchecked().Valid(), TimestampFactory.GetTimestamp());
            var testOrder = new Order("test", "target", "idid", TimestampFactory.Timestamp);
            ResponseAwaiter.AddResponse(confirmation);
            var output = ResponseAwaiter.GetAwaiter(testOrder);
            Assert.IsType<AlreadyKnownChildAwaiter>(output);
        }

        [Fact()]
        public async Task Returns_AlreadyKnownChildAwaiter_true_if_await_for_already_provided_confirmation()
        {
            var testOrder = new Order("test", "target", "idid", TimestampFactory.Timestamp);
            await Task.Delay(_timeout.InMilliseconds / 2);
            var confirmation = new Confirmation("idid", ErrorState.Unchecked().Valid(), TimestampFactory.GetTimestamp());

            ResponseAwaiter.AddResponse(confirmation);
            var awaiter = ResponseAwaiter.GetAwaiter(testOrder);
            var result = await awaiter.RespondedInTime();

            Assert.IsType<AlreadyKnownChildAwaiter>(awaiter);
            Assert.True(result);
        }

        [Fact()]
        public async Task Returns_AlreadyKnownChildAwaiter_false_if_awaiting_already_timeouted_order()
        {
            var testOrder = new Order("test", "target", "idid", TimestampFactory.Timestamp);
            await Task.Delay(_timeout.InMilliseconds + 1).ConfigureAwait(false);
            var confirmation = new Confirmation("idid", ErrorState.Unchecked().Valid(), TimestampFactory.Timestamp);

            ResponseAwaiter.AddResponse(confirmation);
            var awaiter = ResponseAwaiter.GetAwaiter(testOrder);
            var result = await awaiter.RespondedInTime();

            Assert.IsType<AlreadyKnownChildAwaiter>(awaiter);
            Assert.False(result);
        }

        [Fact()]
        public void GetResponseTest()
        {
        }
    }
}