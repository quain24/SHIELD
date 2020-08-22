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
        private DeviceHandlerContext _context;
        private Action<ICommand> _handleReceivedCommandCallback;
        private Task _listeningTask;
        private readonly CommandTranslator _commandTranslator;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;

        public ListeningState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter,
            CommandTranslator commandTranslator)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandTranslator = commandTranslator;
        }

        public void EnterState(DeviceHandlerContext context, Action<ICommand> handleReceivedCommandCallback)
        {
            _context = context;
            _handleReceivedCommandCallback = handleReceivedCommandCallback;
        }

        public void Open()
        {
            Debug.WriteLine("DeviceHandler is already open.");
        }

        public void Close()
        {
            _cts.Cancel();
            _device.Close();
            _context.SetState(new ClosedState(_device, _streamSplitter, _commandTranslator));
        }

        public Task StartListeningAsync()
        {
            if (_listeningTask?.Status == TaskStatus.Running)
            {
                Debug.WriteLine("DeviceContext is already listening");
                return _listeningTask;
            }

            _listeningTask = Listening();
            return Listening();
        }

        public Task StopListeningAsync()
        {
            _cts.Cancel();
            _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator));
            return Task.CompletedTask;
        }

        public Task<bool> SendAsync(ICommand command)
        {
            return _device.SendAsync(_commandTranslator.TranslateFrom(command).ToString());
        }

        private async Task Listening()
        {
            try
            {
                while (!_cts.IsCancellationRequested && _device.IsReady)
                {
                    var data = await _device.ReceiveAsync(_cts.Token).ConfigureAwait(false);

                    foreach (var entry in _streamSplitter.Split(data))
                    {
                        var command = _commandTranslator.TranslateFrom(entry);
                        _handleReceivedCommandCallback(command);
                        _cts.Token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException) when (_device.IsReady)
            {
                Debug.WriteLine("Properly canceled");
                _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator));
            }
            catch
            {
                Close();
                throw;
            }
        }
    }
}