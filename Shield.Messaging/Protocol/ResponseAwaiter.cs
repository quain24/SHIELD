using System;
using System.Collections.Concurrent;
using System.Threading;
using Timeout = Shield.Messaging.Commands.Timeout;

namespace Shield.Messaging.Protocol
{
    public class ResponseAwaiter
    {
        private readonly ConcurrentDictionary<(string, ResponseType), IResponseMessage> _responseBuffer =
            new ConcurrentDictionary<(string, ResponseType), IResponseMessage>();

        private readonly Timeout _timeout;

        private readonly ConcurrentDictionary<(string, ResponseType), CancellationTokenSource> _tokenBuffer =
            new ConcurrentDictionary<(string, ResponseType), CancellationTokenSource>();

        public ResponseAwaiter(Timeout timeout)
        {
            _timeout = timeout;
        }

        public IChildAwaiter GetConfirmationAwaiter(Order order) => GetAwaiterFor(order, ResponseType.Confirmation);

        public IChildAwaiter GetReplyAwaiter(Order order) => GetAwaiterFor(order, ResponseType.Reply);

        private IChildAwaiter GetAwaiterFor(Order order, ResponseType type)
        {
            if (ReplyExists(order, type))
                return new AlreadyKnownChildAwaiter(!IsTimeoutExceeded(order));

            if (IsTimeoutExceeded(order))
                return new AlreadyKnownChildAwaiter(false);

            return CreateNormalAwaiterWithBufferedToken(order, type);
        }

        private bool ReplyExists(Order order, ResponseType type) => _responseBuffer.TryGetValue((order.ID, type), out _);

        private bool IsTimeoutExceeded(Order order) => _timeout.IsExceeded(order.Timestamp);

        private ChildAwaiter CreateNormalAwaiterWithBufferedToken(Order order, ResponseType type)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            if (_tokenBuffer.TryAdd((order.ID, type), cancellationTokenSource))
                return new ChildAwaiter(_timeout, cancellationTokenSource.Token);

            throw new Exception($"Could not create new ChildAwaiter of type \"{Enum.GetName(typeof(ResponseType), type)}\" for \"{order.ID}\" - Cancellation token for that id and type is already used in buffer");
        }

        public void AddConfirmation(IResponseMessage confirmation) => AddResponse(confirmation, ResponseType.Confirmation);

        public void AddReply(IResponseMessage reply) => AddResponse(reply, ResponseType.Reply);

        private void AddResponse(IResponseMessage response, ResponseType type)
        {
            TryBufferResponse(response, type);
            TryInformCorrespondingChildAwaiter(response, type);
        }

        private bool TryBufferResponse(IResponseMessage response, ResponseType type) => _responseBuffer.TryAdd((response.Target, type), response);

        private bool TryInformCorrespondingChildAwaiter(IResponseMessage response, ResponseType type)
        {
            if (_tokenBuffer.TryRemove((response.Target, type), out var cts))
            {
                cts?.Cancel();
                cts?.Dispose();
                return true;
            }

            return false;
        }

        public IResponseMessage GetConfirmationOf(Order order) => GetResponse(order, ResponseType.Confirmation);

        public IResponseMessage GetReplyTo(Order order) => GetResponse(order, ResponseType.Reply);

        private IResponseMessage GetResponse(Order order, ResponseType type)
        {
            _responseBuffer.TryRemove((order.ID, type), out var response);
            return response;
        }

        public IResponseMessage GetReplyFor(Order order)
        {
            _responseBuffer.TryRemove((order.ID, ResponseType.Reply), out var reply);
            return reply;
        }
    }

    public enum ResponseType
    {
        Confirmation,
        Reply
    }
}