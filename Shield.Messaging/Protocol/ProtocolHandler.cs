﻿using Shield.Messaging.Commands;
using Shield.Messaging.Commands.States;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public class ProtocolHandler
    {
        private readonly IDeviceHandler _deviceHandler;
        private readonly CommandTranslator _commandTranslator;
        private readonly ResponseAwaiterDispatch _awaiterDispatch;

        public event EventHandler<Order> OrderReceived;
        public event EventHandler<ErrorMessage> IncomingCommunicationErrorOccurred;

        public ProtocolHandler(IDeviceHandler deviceHandler, CommandTranslator commandTranslator, ResponseAwaiterDispatch awaiterDispatch)
        {
            _deviceHandler = deviceHandler;
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

        public async Task<Confirmation> SendAsync(IConfirmable order)
        {
            if (!_deviceHandler.IsReady || !await _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(order)).ConfigureAwait(false))
                return Confirmation.Create(order.ID, ErrorState.Unchecked().SendFailure());

            if (!await Order().WasConfirmedInTimeAsync(order).ConfigureAwait(false))
                return Confirmation.Create(order.ID, ErrorState.Unchecked().OrderNotConfirmed());

            return Retrieve().ConfirmationOf(order);
        }

        public Task<bool> SendAsync(Confirmation confirmation)
        {
            return _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(confirmation));
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
                OnOrderReceived(_commandTranslator.TranslateToOrder(command));
        }

        protected virtual void OnOrderReceived(Order order) =>
            OrderReceived?.Invoke(this, order);

        protected virtual void OnCommunicationErrorOccurred(ErrorMessage errorMessage) =>
            IncomingCommunicationErrorOccurred?.Invoke(this, errorMessage);
    }
}