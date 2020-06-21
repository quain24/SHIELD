using Shield.Messaging.Commands;
using Shield.Messaging.Devices.DeviceHandlerStates;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices
{
    public class DeviceHandlerContext
    {
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private readonly CommandFactory _commandFactory;

        private IDeviceHandlerState _currentState;
        private readonly SortedDictionary<Timestamp, ICommand> _buffer = new SortedDictionary<Timestamp, ICommand>();

        public EventHandler<ICommand> CommandReceived;

        public DeviceHandlerContext(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandFactory commandFactory)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device), "Passed device cannot be NULL");
            _streamSplitter = streamSplitter ?? throw new ArgumentNullException(nameof(streamSplitter));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            SetState(new ClosedState(_device, _streamSplitter, _commandFactory, _buffer));
        }

        public void SetState(IDeviceHandlerState state)
        {
            if (_currentState == state)
                return;

            if(_currentState != null)
                _currentState.CommandReceived -= CommandReceived;
            _currentState = state ?? throw new ArgumentNullException(nameof(state));
            _currentState.CommandReceived += CommandReceived;
            _currentState.EnterState(this);
        }

        public void Open() => _currentState.Open();

        public void Close() => _currentState.Close();

        public async Task StartListeningAsync() => await _currentState.StartListeningAsync().ConfigureAwait(false);

        public async Task StopListeningAsync() => await _currentState.StopListeningAsync().ConfigureAwait(false);

        public async Task<bool> SendAsync(RawCommand command) => await _currentState.SendAsync(command).ConfigureAwait(false);

        // TODO cancellation, tokens, states, events, acces to bufer etc
        // Implement state pattern in full
    }
}