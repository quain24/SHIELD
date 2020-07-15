using Shield.Timestamps;
using System;
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

        public Task<bool> AwaitResponseAsync(Order order)
        {
            if (_confBuffer.TryRemove(order.ID, out _))
                return Task.FromResult(true);

            if (_timeout.IsExceeded(order.Timestamp))
                return Task.FromResult(false);

            var cts = new CancellationTokenSource();
            if (_ctsBuffer.TryAdd(order.ID, cts))
                return new ChildAwaiter(_timeout, cts).RespondedInTime();

            throw new Exception($"Could not create new ChildAwaiter for \"{order.ID}\" - Cancellation token for that id is already used in buffer");
        }

        public void AddResponse(Confirmation confirmation)
        {
            if (_ctsBuffer.TryRemove(confirmation.Confirms, out CancellationTokenSource cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        private void AddToMonitoring(Order order)
        {
            _buffer.TryAdd(order.Timestamp, order);
        }
    }
}