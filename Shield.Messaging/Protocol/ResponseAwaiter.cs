using Shield.Timestamps;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, Confirmation> _confBuffer = new ConcurrentDictionary<string, Confirmation>();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _ctsBuffer = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly ConcurrentDictionary<string, ChildAwaiter> _awaiterBuffer = new ConcurrentDictionary<string, ChildAwaiter>();

        public ResponseAwaiter(Timeout timeout)
        {
            _timeout = timeout;
            _timer = InitializeTimer();
        }

        private Timer InitializeTimer()
        {
            return new Timer(_, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        private void StartTimer() => _timer.Change(0, _timeout.InMilliseconds);

        private void StopTimer() => _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

        public IChildAwaiter TryAwaitResponseAsync(Order order)
        {
            if (_confBuffer.TryRemove(order.ID, out _))
                return new AlreadyKnownChildAwaiter(true);

            if (_timeout.IsExceeded(order.Timestamp))
                return new AlreadyKnownChildAwaiter(false);

            var awaiter = new ChildAwaiter(_timeout);
            if(_awaiterBuffer.TryAdd(order.ID, awaiter))
                return new ChildAwaiter(_timeout);

            throw new Exception($"Could not create new ChildAwaiter for \"{order.ID}\" - Cancellation token for that id is already used in buffer");
        }

        public void AddResponse(Confirmation confirmation)
        {
            _confBuffer.TryAdd(confirmation.Confirms, confirmation);
            if (_awaiterBuffer.TryRemove(confirmation.Confirms, out ChildAwaiter awaiter))
            {
                awaiter.Interrupt();
            }
        }

        public Confirmation GetResponse(Order order)
        {
            _confBuffer.TryRemove(order.ID, out var confirmation);
            return confirmation;
        }

        private void AddToMonitoring(Order order)
        {
            _buffer.TryAdd(order.Timestamp, order);
        }
    }
}