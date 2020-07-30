using System;
using System.Collections.Concurrent;
using System.Threading;
using Timeout = Shield.Messaging.Commands.Timeout;

namespace Shield.Messaging.Protocol
{
    public class ResponseAwaiter
    {
        private readonly ConcurrentDictionary<string, IResponseMessage> _responseBuffer =
            new ConcurrentDictionary<string, IResponseMessage>();

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenBuffer =
            new ConcurrentDictionary<string, CancellationTokenSource>();

        private readonly Timeout _timeout;

        public static ResponseAwaiter GetNewInstance(Timeout timeout) => new ResponseAwaiter(timeout);

        public ResponseAwaiter(Timeout timeout)
        {
            _timeout = timeout;
        }

        internal IChildAwaiter GetAwaiterFor(Order order)
        {
            if (ReplyExists(order))
                return new AlreadyKnownChildAwaiter(!IsTimeoutExceeded(order));

            if (IsTimeoutExceeded(order))
                return new AlreadyKnownChildAwaiter(false);

            return CreateNormalAwaiterWithBufferedToken(order);
        }

        private bool ReplyExists(Order order) => _responseBuffer.TryGetValue(order.ID, out _);

        private bool IsTimeoutExceeded(Order order) => _timeout.IsExceeded(order.Timestamp);

        private ChildAwaiter CreateNormalAwaiterWithBufferedToken(Order order)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            if (_tokenBuffer.TryAdd(order.ID, cancellationTokenSource))
                return new ChildAwaiter(_timeout, cancellationTokenSource.Token);

            throw new Exception($"Could not create new ChildAwaiter of type for \"{order.ID}\" - Cancellation token for that ID in this instance is already used in buffer");
        }

        internal void AddResponse(IResponseMessage response)
        {
            TryBufferResponse(response);
            TryInformCorrespondingChildAwaiter(response);
        }

        private bool TryBufferResponse(IResponseMessage response) => _responseBuffer.TryAdd(response.Target, response);

        private bool TryInformCorrespondingChildAwaiter(IResponseMessage response)
        {
            if (_tokenBuffer.TryRemove(response.Target, out var cts))
            {
                cts?.Cancel();
                cts?.Dispose();
                return true;
            }

            return false;
        }

        internal IResponseMessage GetResponse(Order order)
        {
            _responseBuffer.TryRemove(order.ID, out var response);
            return response;
        }
    }
}