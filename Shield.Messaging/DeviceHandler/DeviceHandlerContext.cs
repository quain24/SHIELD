using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler.States;
using Shield.Messaging.RawData;
using Shield.Timestamps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.DeviceHandler
{
    public class DeviceHandlerContext : IDeviceHandler
    {
        private IDeviceHandlerState _currentState;
        private readonly SortedDictionary<Timestamp, ICommand> _commandBuffer = new SortedDictionary<Timestamp, ICommand>();
        private readonly ICommunicationDeviceAsync _device;

        public DeviceHandlerContext(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandTranslator commandTranslator)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device), "Passed device cannot be NULL");
            _ = commandTranslator ?? throw new ArgumentNullException(nameof(commandTranslator));
            _ = streamSplitter ?? throw new ArgumentNullException(nameof(streamSplitter));
            Name = device.Name;
            SetState(new ClosedState(device, streamSplitter, commandTranslator));
        }

        public event EventHandler<ICommand> CommandReceived;

        public event EventHandler<ICommand> CommandSent;

        public event EventHandler<ICommand> CommandSendingFailed;

        public string Name { get; }
        public bool IsReady => _device.IsReady;
        public bool IsOpen => _device.IsOpen;
        public bool IsConnected => _device.IsConnected;

        internal void SetState(IDeviceHandlerState newState)
        {
            if (_currentState == newState)
                return;
            _currentState = newState ?? throw new ArgumentNullException(nameof(newState), "New state cannot be NULL.");
            _currentState.EnterState(this, HandleReceivedCommand);
        }

        private void HandleReceivedCommand(ICommand command)
        {
            _commandBuffer.Add(command.Timestamp, command);
            OnCommandReceived(command);
        }

        public void Open() => _currentState.Open();

        public void Close() => _currentState.Close();

        public Task StartListeningAsync() => _currentState.StartListeningAsync();

        public Task StopListeningAsync() => _currentState.StopListeningAsync();

        public async Task<bool> SendAsync(ICommand command)
        {
            if (await _currentState.SendAsync(command).ConfigureAwait(false))
            {
                OnCommandSent(command);
                return true;
            }
            OnCommandSendingFailed(command);
            return false;
        }

        protected virtual void OnCommandReceived(ICommand command) =>
            CommandReceived?.Invoke(this, command);

        protected virtual void OnCommandSent(ICommand command) =>
            CommandSent?.Invoke(this, command);

        protected virtual void OnCommandSendingFailed(ICommand command) =>
            CommandSendingFailed?.Invoke(this, command);
    }
}