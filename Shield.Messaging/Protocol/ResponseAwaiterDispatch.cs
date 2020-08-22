using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public class ResponseAwaiterDispatch : IAwaitingDispatch, IRetrievingDispatch
    {
        private readonly IDictionary<ResponseType, ResponseAwaiter> _responseAwaiters;

        public ResponseAwaiterDispatch(IDictionary<ResponseType, ResponseAwaiter> responseAwaiters)
        {
            _responseAwaiters = CheckResponseAwaiterMap(responseAwaiters)
                ? responseAwaiters
                : throw new ArgumentOutOfRangeException(nameof(responseAwaiters), $"{nameof(responseAwaiters)} cannot be null and there has to be one awaiter per response type");
        }

        private bool CheckResponseAwaiterMap(IDictionary<ResponseType, ResponseAwaiter> awaiters)
        {
            if (awaiters is null) return false;
            return ((ResponseType[])Enum.GetValues(typeof(ResponseType))).All(type => awaiters.ContainsKey(type) && !(awaiters[type] is null));
        }

        public Task<bool> WasConfirmedInTimeAsync(IConfirmable message)
        {
            return _responseAwaiters[ResponseType.Confirmation].GetAwaiterFor(message).HasRespondedInTimeAsync();
        }

        public Task<bool> WasRepliedToInTimeAsync(Order order)
        {
            return _responseAwaiters[ResponseType.Reply].GetAwaiterFor(order).HasRespondedInTimeAsync();
        }

        public Confirmation ConfirmationOf(IConfirmable message) =>
            _responseAwaiters[ResponseType.Confirmation].GetResponse(message) as Confirmation;

        public Reply ReplyTo(Order order) =>
            _responseAwaiters[ResponseType.Reply].GetResponse(order) as Reply;

        public void AddResponse(IResponseMessage message)
        {
            switch (message)
            {
                case Reply reply:
                    _responseAwaiters[ResponseType.Reply].AddResponse(reply);
                    break;

                case Confirmation confirmation:
                    _responseAwaiters[ResponseType.Confirmation].AddResponse(confirmation);
                    break;

                case null:
                    throw new ArgumentNullException(nameof(message), $"Passed {nameof(IResponseMessage)} was null");

                default:
                    throw new ArgumentOutOfRangeException(nameof(message), $"Passed {nameof(message)} type ({message.GetType()}) is unknown to this {nameof(ResponseAwaiterDispatch)} instance and cannot be handled.");
            }
        }
    }

    public enum ResponseType
    {
        Confirmation,
        Reply
    }
}