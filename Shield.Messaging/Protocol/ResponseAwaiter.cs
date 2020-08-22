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

        internal IChildAwaiter GetAwaiterFor(IConfirmable message)
        {
            if (ReplyExists(message))
                return new AlreadyKnownChildAwaiter(!IsTimeoutExceeded(message));

            if (IsTimeoutExceeded(message))
                return new AlreadyKnownChildAwaiter(false);

            return CreateNormalAwaiterWithBufferedToken(message);
        }

        private bool ReplyExists(IConfirmable message) => _responseBuffer.TryGetValue(message.ID, out _);

        private bool IsTimeoutExceeded(IConfirmable order) => _timeout.IsExceeded(order.Timestamp);

        private ChildAwaiter CreateNormalAwaiterWithBufferedToken(IConfirmable message)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            if (_tokenBuffer.TryAdd(message.ID, cancellationTokenSource))
                return new ChildAwaiter(_timeout, cancellationTokenSource.Token);

            throw new Exception($"Could not create new ChildAwaiter of type for \"{message.ID}\" - Cancellation token for that ID in this instance is already used in buffer");
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

        internal IResponseMessage GetResponse(IConfirmable order)
        {
            _responseBuffer.TryRemove(order.ID, out var response);
            return response;
        }
    }
}