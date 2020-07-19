using System;
using System.Collections.Concurrent;
using System.Threading;
using Timeout = Shield.Messaging.Commands.Timeout;

namespace Shield.Messaging.Protocol
{
    public class ResponseAwaiter
    {
        private readonly Timeout _timeout;

        private readonly ConcurrentDictionary<string, IResponseMessage> _responseBuffer = new ConcurrentDictionary<string, IResponseMessage>();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenBuffer = new ConcurrentDictionary<string, CancellationTokenSource>();

        public ResponseAwaiter(Timeout timeout)
        {
            _timeout = timeout;
        }

        public IChildAwaiter GetAwaiterFor(Order order)
        {
            if (_responseBuffer.TryGetValue(order.ID, out _))
                return _timeout.IsExceeded(order.Timestamp)
                    ? new AlreadyKnownChildAwaiter(false)
                    : new AlreadyKnownChildAwaiter(true);

            if (_timeout.IsExceeded(order.Timestamp))
                return new AlreadyKnownChildAwaiter(false);

            var cancellationTokenSource = new CancellationTokenSource();
            if (_tokenBuffer.TryAdd(order.ID, cancellationTokenSource))
                return new ChildAwaiter(_timeout, cancellationTokenSource.Token);

            throw new Exception($"Could not create new ChildAwaiter for \"{order.ID}\" - Cancellation token for that id is already used in buffer");
        }

        public void AddResponse(IResponseMessage response)
        {
            _responseBuffer.TryAdd(response.Target, response);
            if (_tokenBuffer.TryRemove(response.Target, out CancellationTokenSource cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        public IResponseMessage GetResponse(Order order)
        {
            _responseBuffer.TryRemove(order.ID, out var response);
            return response;
        }
    }
}