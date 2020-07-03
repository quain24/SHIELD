using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Shield.Messaging.Commands;
using Shield.Messaging.RawData;

namespace Shield.Messaging.DeviceHandler.States
{
    public class ListeningState : IDeviceHandlerState
    {
        private readonly IDictionary _buffer;
        private readonly CommandTranslator _commandTranslator;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private DeviceHandlerContext _context;

        public ListeningState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter,
            CommandTranslator commandTranslator, IDictionary buffer)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandTranslator = commandTranslator;
            _buffer = buffer;
        }

        public event EventHandler<ICommand> CommandReceived;

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
            _context.SetState(new ClosedState(_device, _streamSplitter, _commandTranslator, _buffer));
        }

        public Task StartListeningAsync()
        {
            Debug.WriteLine("DeviceContext is already listening");
            return Task.CompletedTask;
        }

        public Task StopListeningAsync()
        {
            _cts.Cancel();
            _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator, _buffer));
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
                        _buffer.Add(command.Timestamp, command);
                        OnCommandReceived(this, command);
                        _cts.Token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException) when (_device.IsReady)
            {
                Debug.WriteLine("Properly canceled");
                _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator, _buffer));
            }
            catch
            {
                Close();
                throw;
            }
        }

        private void OnCommandReceived(object sender, ICommand e)
        {
            CommandReceived?.Invoke(sender, e);
        }
    }
}