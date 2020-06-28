using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices.DeviceHandlerStates
{
    public class ListeningState : IDeviceHandlerState
    {
        private DeviceHandlerContext _context;
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private readonly CommandFactory _commandFactory;
        private readonly IDictionary _buffer;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ListeningState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter,
            CommandFactory commandFactory, IDictionary buffer)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandFactory = commandFactory;
            _buffer = buffer;
        }

        public event EventHandler<ICommand> CommandReceived;

        public void EnterState(DeviceHandlerContext context)
        {
            _context = context;
        }

        internal async Task Listening()
        {
            try
            {
                while (!_cts.IsCancellationRequested && _device.IsReady)
                {
                    string data = await _device.ReceiveAsync(_cts.Token).ConfigureAwait(false);

                    foreach (var entry in _streamSplitter.Split(data))
                    {
                        var command = _commandFactory.TranslateFrom(entry);
                        _buffer.Add(command.Timestamp, command);
                        OnCommandReceived(this, command);
                        _cts.Token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException) when (_device.IsReady)
            {
                Debug.WriteLine("Properly canceled");
                _context.SetState(new OpenState(_device, _streamSplitter, _commandFactory, _buffer));
            }
            catch
            {
                Close();
                throw;
            }
        }

        public void Open() => Debug.WriteLine("DeviceHandler is already open.");

        public void Close()
        {
            _cts.Cancel();
            _device.Close();
            _context.SetState(new ClosedState(_device, _streamSplitter, _commandFactory, _buffer));
        }

        public Task StartListeningAsync()
        {
            Debug.WriteLine("DeviceContext is already listening");
            return Task.CompletedTask;
        }

        public Task StopListeningAsync()
        {
            _cts.Cancel();
            _context.SetState(new OpenState(_device, _streamSplitter, _commandFactory, _buffer));
            return Task.CompletedTask;
        }

        public async Task<bool> SendAsync(RawCommand command) =>
            await _device.SendAsync(command.ToString()).ConfigureAwait(false);

        private void OnCommandReceived(object sender, ICommand e) => CommandReceived?.Invoke(sender, e);
    }
}