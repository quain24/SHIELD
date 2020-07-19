using Shield.Timestamps;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Timeout = Shield.Messaging.Commands.Timeout;

namespace Shield.Messaging.Protocol
{
    public class ResponseAwaiter
    {
        private readonly Timeout _timeout;
        private readonly Timer _timer;

        private readonly ConcurrentDictionary<Timestamp, Order> _buffer = new ConcurrentDictionary<Timestamp, Order>();
        private readonly ConcurrentDictionary<string, IResponseMessage> _responseBuffer = new ConcurrentDictionary<string, IResponseMessage>();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _ctsBuffer = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly ConcurrentDictionary<string, ChildAwaiter> _awaiterBuffer = new ConcurrentDictionary<string, ChildAwaiter>();

        public ResponseAwaiter(Timeout timeout)
        {
            _timeout = timeout;
            // _timer = InitializeTimer();
        }

        //private Timer InitializeTimer()
        //{
        //    return new Timer(_, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        //}

        //private void StartTimer() => _timer.Change(0, _timeout.InMilliseconds);

        //private void StopTimer() => _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

        public IChildAwaiter GetAwaiter(Order order)
        {
            if (_responseBuffer.TryRemove(order.ID, out var _))
                return _timeout.IsExceeded(order.Timestamp)
                    ? new AlreadyKnownChildAwaiter(false)
                    : new AlreadyKnownChildAwaiter(true);

            if (_timeout.IsExceeded(order.Timestamp))
                return new AlreadyKnownChildAwaiter(false);

            var awaiter = new ChildAwaiter(_timeout);
            if(_awaiterBuffer.TryAdd(order.ID, awaiter))
                return new ChildAwaiter(_timeout);

            throw new Exception($"Could not create new ChildAwaiter for \"{order.ID}\" - Cancellation token for that id is already used in buffer");
        }

        public void AddResponse(IResponseMessage response)
        {
            _responseBuffer.TryAdd(response.Target, response);
            if (_awaiterBuffer.TryRemove(response.Target, out ChildAwaiter awaiter))
            {
                awaiter.Interrupt();
            }
        }

        public IResponseMessage GetResponse(Order order)
        {
            _responseBuffer.TryRemove(order.ID, out var response);
            return response;
        }

        private void AddToMonitoring(Order order)
        {
            _buffer.TryAdd(order.Timestamp, order);
        }
    }
}