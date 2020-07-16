using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Timeout = Shield.Messaging.Commands.Timeout;

namespace Shield.Messaging.Protocol
{
    public class ChildAwaiter : IChildAwaiter
    {
        private readonly Timeout _timeout;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ChildAwaiter(Timeout timeout)
        {
            _timeout = timeout;
        }

        public async Task<bool> AwaitResponseAsync()
        {
            try
            {
                await Task.Delay(_timeout.InMilliseconds, _cts.Token).ConfigureAwait(false);
                Debug.Write("Timeout reached.");
                return false;
            }
            catch (OperationCanceledException)
            {
                Debug.Write("Response in good time");
                return true;
            }
            finally
            {
                _cts.Dispose();
            }
        }

        public void Interrupt()
        {
            _cts?.Cancel();
        }
    }
}