﻿using Shield.Messaging.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.SlaveUnits
{
    public abstract class AbstractSlaveUnit
    {
        private readonly ProtocolHandler _handler;

        protected AbstractSlaveUnit(ProtocolHandler handler)
        {
            _handler = handler;
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public State Status { private set; get; }

        private Task<bool> SendAsync(Order order)
        {
            return _handler.SendAsync(order);
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

        protected abstract IEnumerable<Func<object, object>> MethodsInvokableByOrders();

        public enum State
        {
            Inactive,
            Sending,
            Receiving,
            Busy,
            Error
        }
    }
}