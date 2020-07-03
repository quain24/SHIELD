using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler.States;
using Shield.Messaging.RawData;

namespace Shield.Messaging.DeviceHandler
{
    public class DeviceHandlerContext
    {
        private IDeviceHandlerState _currentState;
        private readonly SortedDictionary<Timestamp, ICommand> _buffer = new SortedDictionary<Timestamp, ICommand>();

        public DeviceHandlerContext(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandTranslator commandTranslator)
        {
            _ = commandTranslator ?? throw new ArgumentNullException(nameof(commandTranslator));
            _ = device ?? throw new ArgumentNullException(nameof(device), "Passed device cannot be NULL");
            _ = streamSplitter ?? throw new ArgumentNullException(nameof(streamSplitter));
            Name = device.Name;
            SetState(new ClosedState(device, streamSplitter, commandTranslator, _buffer));
        }

        public EventHandler<ICommand> CommandReceived;

        public string Name { get; }

        internal void SetState(IDeviceHandlerState newState)
        {
            if (_currentState == newState)
                return;
            SwapState(newState).EnterState(this);
        }

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

        public Task StartListeningAsync() => _currentState.StartListeningAsync();

        public Task StopListeningAsync() => _currentState.StopListeningAsync();

        public Task<bool> SendAsync(ICommand command) => _currentState.SendAsync(command);

        // TODO cancellation, tokens, states, events, access to buffer etc
    }
}