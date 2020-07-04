using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler.States;
using Shield.Messaging.Extensions;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.DeviceHandler
{
    public class DeviceHandlerContext
    {
        private readonly ConfirmationFactory _confirmationFactory;
        private IDeviceHandlerState _currentState;
        private readonly SortedDictionary<Timestamp, ICommand> _commandBuffer = new SortedDictionary<Timestamp, ICommand>();
        private readonly SortedDictionary<Timestamp, ICommand> _confirmationBuffer = new SortedDictionary<Timestamp, ICommand>();

        public DeviceHandlerContext(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandTranslator commandTranslator, ConfirmationFactory confirmationFactory)
        {
            _confirmationFactory = confirmationFactory;
            _ = commandTranslator ?? throw new ArgumentNullException(nameof(commandTranslator));
            _ = device ?? throw new ArgumentNullException(nameof(device), "Passed device cannot be NULL");
            _ = streamSplitter ?? throw new ArgumentNullException(nameof(streamSplitter));
            Name = device.Name;
            SetState(new ClosedState(device, streamSplitter, commandTranslator, HandleReceivedCommandAsync));
        }

        public EventHandler<ICommand> CommandReceived;
        public EventHandler<ICommand> ConfirmationReceived;

        public string Name { get; }

        internal void SetState(IDeviceHandlerState newState)
        {
            if (_currentState == newState)
                return;
            _currentState = newState ?? throw new ArgumentNullException(nameof(newState), "New state cannot be NULL.");
            _currentState.EnterState(this);
        }

        public void Open() => _currentState.Open();

        public void Close() => _currentState.Close();

        public Task StartListeningAsync() => _currentState.StartListeningAsync();

        public Task StopListeningAsync() => _currentState.StopListeningAsync();

        public Task<bool> SendAsync(ICommand command) => _currentState.SendAsync(command);

        private async Task HandleReceivedCommandAsync(ICommand command)
        {
            if (command.IsConfirmation())
            {
                _confirmationBuffer.Add(command.Timestamp, command);
                OnConfirmationReceived(this, command);
            }
            else
            {
                Validate(command);
                await SendAsync(_confirmationFactory.GetConfirmationFor(command)).ConfigureAwait(false);
                _commandBuffer.Add(command.Timestamp, command);
                OnCommandReceived(this, command);
            }
        }

        private bool Validate(ICommand command) => command.IsValid;

        public virtual void OnCommandReceived(object sender, ICommand command) =>
            CommandReceived?.Invoke(sender, command);

        public virtual void OnConfirmationReceived(object sender, ICommand command) =>
            ConfirmationReceived?.Invoke(sender, command);
    }
}