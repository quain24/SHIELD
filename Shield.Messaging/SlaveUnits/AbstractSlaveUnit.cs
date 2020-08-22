using Shield.Messaging.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.SlaveUnits
{
    public abstract class AbstractSlaveUnit : ISlaveUnit
    {
        private readonly ProtocolHandler _handler;
        private readonly OrderFactory _orderFactory;

        protected AbstractSlaveUnit(ProtocolHandler handler, OrderFactory orderFactory)
        {
            _handler = handler;
            _orderFactory = orderFactory;
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public bool CanHandle(Order order)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> SendAsync(Order order)
        {
            var d = await _handler.SendAsync(order);
            return d.IsValid;
        }

        protected virtual async Task<(bool isSuccess, Confirmation confirmation)> TrySendAndAwaitConfirmationAsync(Order order)
        {
            if (await SendAsync(order).ConfigureAwait(false) is false ||
                await _handler.Order().WasConfirmedInTimeAsync(order).ConfigureAwait(false) is false)
                return (false, null);

            return (true, _handler.Retrieve().ConfirmationOf(order));
        }

        protected virtual async Task<(bool isSuccess, Reply reply)> TrySendAndAwaitReplyAsync(Order order)
        {
            if (await SendAsync(order).ConfigureAwait(false) is false ||
                await _handler.Order().WasRepliedToInTimeAsync(order).ConfigureAwait(false) is false)
                return (false, null);

            return (true, _handler.Retrieve().ReplyTo(order));
        }
    }
}