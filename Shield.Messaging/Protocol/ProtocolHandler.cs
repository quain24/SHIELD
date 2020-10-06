using Shield.Exceptions;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.States;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Extensions;
using Shield.Messaging.Protocol.DataPacks;
using Shield.Messaging.Units.SlaveUnits;
using System;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public class ProtocolHandler
    {
        private readonly IDeviceHandler _deviceHandler;
        private readonly ConfirmationFactory _confirmationFactory;
        private readonly ReplyFactory _replyFactory;
        private readonly OrderFactory _orderFactory;
        private readonly CommandTranslator _commandTranslator;
        private readonly ResponseAwaiterDispatch _awaiterDispatch;

        public event EventHandler<ErrorMessage> IncomingCommunicationErrorOccurred;

        public event EventHandler<Order> OrderReceived;

        private Func<Order, Task> _orderReceivedAction = _ => Task.CompletedTask;

        public ProtocolHandler(IDeviceHandler deviceHandler, ConfirmationFactory confirmationFactory, ReplyFactory replyFactory, OrderFactory orderFactory,
            CommandTranslator commandTranslator, ResponseAwaiterDispatch awaiterDispatch)
        {
            _deviceHandler = deviceHandler;
            _confirmationFactory = confirmationFactory;
            _replyFactory = replyFactory;
            _orderFactory = orderFactory;
            _commandTranslator = commandTranslator;
            _awaiterDispatch = awaiterDispatch;
            _deviceHandler.CommandReceived += OnCommandReceived;
        }

        public bool IsOpen => _deviceHandler.IsOpen;
        public bool IsConnected => _deviceHandler.IsConnected;
        public bool IsReady => _deviceHandler.IsReady;

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

        public void AddOrderReceivedHandler(Func<Order, Task> informationDelegate) =>
            _orderReceivedAction += informationDelegate;

        public void RemoveOrderReceivedHandler(Func<Order, Task> informationDelegate) =>
            _orderReceivedAction -= informationDelegate;

        public async Task<bool> Confirm(IConfirmable message, ErrorState errors = null)
        {
            var state = errors ?? ErrorState.Unchecked().Valid();
            var confirmation = _confirmationFactory.Create(message, state);

            return await SendAsync(confirmation).ConfigureAwait(false);
        }

        public async Task<Confirmation> ReplyTo(Order order, IDataPack dataPack)
        {
            var reply = _replyFactory.Create(order, dataPack);
            return await SendAsync(reply).ConfigureAwait(false);
        }

        public Task<Confirmation> SendAsync(AbstractSlaveUnit sender, string targetCommand, IDataPack data = null)
        {
            _ = sender ?? throw new ArgumentNullException(nameof(sender),
                $"Unit has to be a valid child of {nameof(AbstractSlaveUnit)}.");
            if (string.IsNullOrEmpty(targetCommand))
                throw new ArgumentNullException(nameof(targetCommand), "Target command cannot be empty!");

            var order = _orderFactory.Create(targetCommand, sender.ID, data ?? EmptyDataPackSingleton.GetInstance());
            return SendAsync(order);
        }

        public async Task<Confirmation> SendAsync(IConfirmable order)
        {
            var errorState = ErrorState.Unchecked().Valid();
            try
            {
                if (!IsReady)
                    errorState = errorState.DeviceDisconnected();
                else if (!await _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(order)).ConfigureAwait(false))
                    errorState = errorState.SendFailure();
                else if (!await Order().WasConfirmedInTimeAsync(order).ConfigureAwait(false))
                    errorState = errorState.OrderNotConfirmed();
            }
            catch (DeviceDisconnectedException)
            {
                errorState = errorState.DeviceDisconnected();
            }

            _awaiterDispatch.AddResponse(_confirmationFactory.Create(order, errorState));
            return Retrieve().ConfirmationOf(order);
        }

        internal Task<bool> SendAsync(Confirmation confirmation)
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