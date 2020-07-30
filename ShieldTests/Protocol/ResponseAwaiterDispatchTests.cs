using Xunit;
using Shield.Messaging.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Commands;
using ShieldTests.Protocol;
using Xunit.Abstractions;

namespace Shield.Messaging.Protocol.Tests
{
    public class ResponseAwaiterDispatchTests
    {
        private readonly ITestOutputHelper _output;

        public ResponseAwaiterDispatchTests(ITestOutputHelper output)
        {
            _output = output;
            Setup();
        }

        public ResponseAwaiterDispatch Dispatch;
        public Order NormalOrder;
        public Confirmation NormalConfirmation;
        public Reply NormalReply;


        public void Setup()
        {
            
            Dispatch = new ResponseAwaiterDispatch(ResponseAwaiterDispatchTestObjects.GetProperAwaitersDictionary());
            NormalOrder = ProtocolTestObjects.GetNormalOrder();
            NormalConfirmation = ProtocolTestObjects.GetNormalConfirmation();
            NormalReply = ProtocolTestObjects.GetNormalReply();
        }

        [Fact()]
        public void Will_throw_Exception_if_is_given_null_instead_of_proper_awaiter_collection()
        {
            var exception = Record.Exception(() => new ResponseAwaiterDispatch(null));
            
            _output.WriteLine($"Given message: {exception?.Message ?? "Tried to get message but exception was null."}");
            
            Assert.IsType<ArgumentOutOfRangeException>(exception);

        }

        [Fact()]
        public void Will_throw_if_is_given_not_complete_list_of_needed_awaiters()
        {
            var exception = Record.Exception(() =>
                new ResponseAwaiterDispatch(ResponseAwaiterDispatchTestObjects.GetIAwatersDictionaryWithoutReply()));

            _output.WriteLine($"Given message: {exception?.Message ?? "Tried to get message but exception was null."}");

            Assert.IsType<ArgumentOutOfRangeException>(exception);

        }

        [Fact()]
        public async Task Gives_bool_when_asked_about_reply_to_order()
        {
            var result = await Dispatch.RepliedToInTimeAsync(NormalOrder).ConfigureAwait(false);

            Assert.IsType<bool>(result);
        }

        [Fact()]
        public async Task Gives_bool_when_asked_about_confirmation_to_order()
        {
            var result = await Dispatch.ConfirmedInTimeAsync(NormalOrder).ConfigureAwait(false);

            Assert.IsType<bool>(result);
        }

        [Fact()]
        public void ReplyToTest()
        {

        }

        [Fact()]
        public void AddResponseTest()
        {

        }
    }
}