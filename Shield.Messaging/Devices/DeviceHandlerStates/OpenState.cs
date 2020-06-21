using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices.DeviceHandlerStates
{
    public sealed class OpenState : IDeviceHandlerState
    {
        private DeviceHandlerContext _context;
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private readonly CommandFactory _commandFactory;
        private readonly IDictionary _buffer;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public event EventHandler<ICommand> CommandReceived;

        public OpenState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandFactory commandFactory, IDictionary buffer)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandFactory = commandFactory;
            _buffer = buffer;
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
            _context.SetState(new ClosedState(_device, _streamSplitter, _commandFactory, _buffer));
        }

        public Task StartListeningAsync()
        {
            var state = new ListeningState(_device, _streamSplitter, _commandFactory, _buffer);
            _context.SetState(state);
            return state.Listening();
        }

        public Task StopListeningAsync()
        {
            Debug.WriteLine("DeviceHandlerContext is not currently listening");
            return Task.CompletedTask;
        }

        public async Task<bool> SendAsync(RawCommand command)
        {
            return await _device.SendAsync(command.ToString(), _cts.Token).ConfigureAwait(false);
        }
    }
}