using Shield.Messaging.Commands;
using Shield.Messaging.Devices.DeviceHandlerStates;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices
{
    public class DeviceHandlerContext
    {
        private IDeviceHandlerState _currentState;
        private readonly SortedDictionary<Timestamp, ICommand> _buffer = new SortedDictionary<Timestamp, ICommand>();

        public DeviceHandlerContext(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandFactory commandFactory)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device), "Passed device cannot be NULL");
            _ = streamSplitter ?? throw new ArgumentNullException(nameof(streamSplitter));
            _ = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            Name = device.Name;
            SetState(new ClosedState(device, streamSplitter, commandFactory, _buffer));
        }

        public EventHandler<ICommand> CommandReceived;

        internal void SetState(IDeviceHandlerState newState)
        {
            if (_currentState == newState)
                return;
            SwapState(newState).EnterState(this);
        }

        public string Name { get; }

        private IDeviceHandlerState SwapState(IDeviceHandlerState newState)
        {
            if (_currentState != null)
                _currentState.CommandReceived -= CommandReceived;
            _currentState = newState ?? throw new ArgumentNullException(nameof(newState));
            _currentState.CommandReceived += CommandReceived;
            return _currentState;
        }

        public void Open() => _currentState.Open();

        public void Close() => _currentState.Close();

        public async Task StartListeningAsync() => await _currentState.StartListeningAsync().ConfigureAwait(false);

        public async Task StopListeningAsync() => await _currentState.StopListeningAsync().ConfigureAwait(false);

        public async Task<bool> SendAsync(RawCommand command) => await _currentState.SendAsync(command).ConfigureAwait(false);

        // TODO cancellation, tokens, states, events, access to buffer etc
    }
}