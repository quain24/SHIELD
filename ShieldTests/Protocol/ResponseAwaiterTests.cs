using System;
using System.Diagnostics;
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
        private Order StandardOrder => new Order("test", "target", "idid", TimestampFactory.Timestamp);
        private Confirmation StandardConfirmation => new Confirmation("idid", ErrorState.Unchecked().Valid(), TimestampFactory.Timestamp);
        private IResponseMessage StdConfirmationAsIResponse => StandardConfirmation;

        private void Setup()
        {
            _timeout = new Timeout(_timeoutInMilliseconds);
            ResponseAwaiter = new ResponseAwaiter(_timeout);
        }

        [Fact()]
        public void Returns_child_awaiter_when_await_response_called_for_not_already_responded_order()
        {
            var output = ResponseAwaiter.GetAwaiterFor(StandardOrder);

            Assert.IsType<ChildAwaiter>(output);
        }

        [Fact()]
        public void Returns_AlreadyKnownAwaiter_when_used_to_await_for_already_provided_confirmation()
        {
            ResponseAwaiter.AddResponse(StandardConfirmation);
            var output = ResponseAwaiter.GetAwaiterFor(StandardOrder);
            Assert.IsType<AlreadyKnownChildAwaiter>(output);
        }

        [Fact()]
        public async Task Returns_AlreadyKnownChildAwaiter_true_if_await_for_already_provided_confirmation()
        {
            var testOrder = StandardOrder;
            await Task.Delay(_timeout.InMilliseconds / 2);

            ResponseAwaiter.AddResponse(StandardConfirmation);
            var awaiter = ResponseAwaiter.GetAwaiterFor(testOrder);
            var result = await awaiter.HasRespondedInTimeAsync();

            Assert.IsType<AlreadyKnownChildAwaiter>(awaiter);
            Assert.True(result);
        }

        [Fact()]
        public async Task Returns_AlreadyKnownChildAwaiter_false_if_awaiting_already_timeouted_order()
        {
            var testOrder = StandardOrder;
            await Task.Delay(_timeout.InMilliseconds + 1).ConfigureAwait(false);

            ResponseAwaiter.AddResponse(StandardConfirmation);
            var awaiter = ResponseAwaiter.GetAwaiterFor(testOrder);
            var result = await awaiter.HasRespondedInTimeAsync();

            Assert.IsType<AlreadyKnownChildAwaiter>(awaiter);
            Assert.False(result);
        }

        [Fact()]
        public async Task Returns_ChildAwaiter_true_when_response_came_in_time()
        {
            var testOrder = StandardOrder;
            #pragma warning disable 4014
            Task.Run(async () =>
            {
                await Task.Delay(_timeoutInMilliseconds / 2).ConfigureAwait(false);
                ResponseAwaiter.AddResponse(StandardConfirmation);
            }).ConfigureAwait(false);
            #pragma warning restore 4014

            var awaiter = ResponseAwaiter.GetAwaiterFor(testOrder);
            var result = await awaiter.HasRespondedInTimeAsync().ConfigureAwait(false);

            Assert.IsType<ChildAwaiter>(awaiter);
            Assert.True(result);
        }

        [Fact()]
        public async Task Returns_ChildAwaiter_false_when_response_came_after_allowed_timeout()
        {
            var testOrder = StandardOrder;
            #pragma warning disable 4014
            Task.Run(async () =>
            {
                await Task.Delay(_timeoutInMilliseconds + 50).ConfigureAwait(false);
                ResponseAwaiter.AddResponse(StandardConfirmation);
            }).ConfigureAwait(false);
            #pragma warning restore 4014

            var awaiter = ResponseAwaiter.GetAwaiterFor(testOrder);
            var result = await awaiter.HasRespondedInTimeAsync().ConfigureAwait(false);

            Assert.IsType<ChildAwaiter>(awaiter);
            Assert.False(result);
        }

        [Fact()]
        public async Task Gets_proper_response_when_response_came_in_time()
        {
            var testOrder = StandardOrder;
            #pragma warning disable 4014
            Task.Run(async () =>
            {
                await Task.Delay(_timeoutInMilliseconds / 2).ConfigureAwait(false);
                ResponseAwaiter.AddResponse(StandardConfirmation);
            }).ConfigureAwait(false);
            #pragma warning restore 4014

            var awaiter = ResponseAwaiter.GetAwaiterFor(testOrder);
            var result = await awaiter.HasRespondedInTimeAsync().ConfigureAwait(false);
            IResponseMessage response = null;
            if (result)
                response = ResponseAwaiter.GetResponse(testOrder);
            Assert.NotNull(response);
            Assert.IsAssignableFrom<Confirmation>(response);
            Assert.Equal(response.Target, testOrder.ID);
        }

        [Fact()]
        public async Task Will_throw_exception_when_given_same_id_order_to_await_for()
        {
            var testOrder = StandardOrder;

            var awaiter = ResponseAwaiter.GetAwaiterFor(testOrder);
            var exception = Record.Exception(() => ResponseAwaiter.GetAwaiterFor(testOrder));

            _output.WriteLine($"Given message: {exception?.Message}");
            Assert.IsType<Exception>(exception);
        }
    }
}