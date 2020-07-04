using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.Messaging.DeviceHandler.States
{
    public class ListeningState : IDeviceHandlerState
    {
        private readonly CommandTranslator _commandTranslator;
        private readonly Func<ICommand, Task> _handleReceivedCommandCallbackAsync;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private DeviceHandlerContext _context;

        public ListeningState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter,
            CommandTranslator commandTranslator, Func<ICommand, Task> handleReceivedCommandCallbackAsync)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandTranslator = commandTranslator;
            _handleReceivedCommandCallbackAsync = handleReceivedCommandCallbackAsync;
        }

        public void EnterState(DeviceHandlerContext context)
        {
            _context = context;
        }

        public void Open()
        {
            Debug.WriteLine("DeviceHandler is already open.");
        }

        public void Close()
        {
            _cts.Cancel();
            _device.Close();
            _context.SetState(new ClosedState(_device, _streamSplitter, _commandTranslator, _handleReceivedCommandCallbackAsync));
        }

        public Task StartListeningAsync()
        {
            Debug.WriteLine("DeviceContext is already listening");
            return Task.CompletedTask;
        }

        public Task StopListeningAsync()
        {
            _cts.Cancel();
            _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator, _handleReceivedCommandCallbackAsync));
            return Task.CompletedTask;
        }

        public Task<bool> SendAsync(ICommand command)
        {
            return _device.SendAsync(_commandTranslator.TranslateFrom(command).ToString());
        }

        internal async Task Listening()
        {
            try
            {
                while (!_cts.IsCancellationRequested && _device.IsReady)
                {
                    var data = await _device.ReceiveAsync(_cts.Token).ConfigureAwait(false);

                    foreach (var entry in _streamSplitter.Split(data))
                    {
                        var command = _commandTranslator.TranslateFrom(entry);
                        await _handleReceivedCommandCallbackAsync(command).ConfigureAwait(false);
                        _cts.Token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException) when (_device.IsReady)
            {
                Debug.WriteLine("Properly canceled");
                _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator, _handleReceivedCommandCallbackAsync));
            }
            catch
            {
                Close();
                throw;
            }
        }
    }
}