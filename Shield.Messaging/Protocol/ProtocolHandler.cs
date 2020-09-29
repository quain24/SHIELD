using Shield.Exceptions;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.States;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Extensions;
using System;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public class ProtocolHandler
    {
        private readonly IDeviceHandler _deviceHandler;
        private readonly ConfirmationFactory _confirmationFactory;
        private readonly CommandTranslator _commandTranslator;
        private readonly ResponseAwaiterDispatch _awaiterDispatch;

        public event EventHandler<Order> OrderReceived;

        public event EventHandler<ErrorMessage> IncomingCommunicationErrorOccurred;

        private Action<Order> _orderReceivedAction;

        public ProtocolHandler(IDeviceHandler deviceHandler, ConfirmationFactory confirmationFactory, CommandTranslator commandTranslator, ResponseAwaiterDispatch awaiterDispatch)
        {
            _deviceHandler = deviceHandler;
            _confirmationFactory = confirmationFactory;
            _commandTranslator = commandTranslator;
            _awaiterDispatch = awaiterDispatch;
            _deviceHandler.CommandReceived += OnCommandReceived;
        }

        public bool IsOpen => _deviceHandler.IsOpen;
        public bool IsConnected => _deviceHandler.IsConnected;
        public bool IsReady => _deviceHandler.IsReady;

        public void AddOrderReceivedHandler(Action<Order> informationDelegate) => _orderReceivedAction += informationDelegate;

        public void RemoveOrderReceivedHandler(Action<Order> informationDelegate) => _orderReceivedAction -= informationDelegate;

        public void Open()
        {
            if (!_deviceHandler.IsOpen)
                _deviceHandler.Open();
            if (!_deviceHandler.IsConnected)
                _deviceHandler.StartListeningAsync();
        }

        public void Close()
        {
            if (_deviceHandler.IsOpen)
                _deviceHandler.Close();
        }

        public async Task<Confirmation> SendAsync(IConfirmable order)
        {
            try
            {
                if (!IsReady)
                    return _confirmationFactory.Create(order, ErrorState.Unchecked().DeviceDisconnected());

                if (!await _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(order)).ConfigureAwait(false))
                    return _confirmationFactory.Create(order, ErrorState.Unchecked().SendFailure());

                if (!await Order().WasConfirmedInTimeAsync(order).ConfigureAwait(false))
                    return _confirmationFactory.Create(order, ErrorState.Unchecked().OrderNotConfirmed());
            }
            catch (DeviceDisconnectedException)
            {
                return _confirmationFactory.Create(order, ErrorState.Unchecked().DeviceDisconnected());
            }

            return Retrieve().ConfirmationOf(order);
        }

        public Task<bool> SendAsync(Confirmation confirmation)
        {
            try
            {
                return _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(confirmation));
            }
            catch (DeviceDisconnectedException)
            {
                return Task.FromResult(false);
            }
        }

        public IAwaitingDispatch Order() => _awaiterDispatch;

        public IRetrievingDispatch Retrieve() => _awaiterDispatch;

        protected virtual void OnCommandReceived(object sender, ICommand command)
        {
            if (!command.IsValid) // protocol failure
            {
                OnCommunicationErrorOccurred(_commandTranslator.TranslateToErrorMessage(command));
            }

            if (command.IsConfirmation())
                _awaiterDispatch.AddResponse(_commandTranslator.TranslateToConfirmation(command));
            else if (command.IsReply())
                _awaiterDispatch.AddResponse(_commandTranslator.TranslateToReply(command));
            else
            {
                var order = _commandTranslator.TranslateToOrder(command);
                OnOrderReceived(order);
                _orderReceivedAction(order);
            }
        }

        protected virtual void OnOrderReceived(Order order) =>
            OrderReceived?.Invoke(this, order);

        protected virtual void OnCommunicationErrorOccurred(ErrorMessage errorMessage) =>
            IncomingCommunicationErrorOccurred?.Invoke(this, errorMessage);
    }
}