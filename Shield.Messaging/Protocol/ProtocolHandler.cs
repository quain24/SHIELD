using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public class ProtocolHandler
    {
        private readonly DeviceHandlerContext _deviceHandler;
        private readonly CommandTranslator _commandTranslator;
        private readonly ResponseAwaiter _responseAwaiter;

        public event EventHandler<Order> OrderReceived;
        public event EventHandler<ErrorMessage> CommunicationErrorOccured;

        public ProtocolHandler(DeviceHandlerContext deviceHandler, CommandTranslator commandTranslator, ResponseAwaiter responseAwaiter)
        {
            _deviceHandler = deviceHandler;
            _commandTranslator = commandTranslator;
            _responseAwaiter = responseAwaiter;
            _deviceHandler.CommandReceived += OnCommandReceived;
        }

        public Task<bool> SendAsync(Order order)
        {
            return _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(order));
        }

        public Task<bool> SendAsync(Confirmation confirmation)
        {
            return _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(confirmation));
        }

        public Task<bool> SendAsync(Reply reply)
        {
            return _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(reply));
        }

        public async Task<Confirmation> AwaitConfirmationOfAsync(Order order)
        {
            var isConfirmed = await WasRespondedToInTimeAsync(order).ConfigureAwait(false);

            return isConfirmed && IsResponseConfirmation(order, out var confirmation)
                ? confirmation
                : null;
        }

        public async Task<Reply> AwaitReplyToAsync(Order order)
        {
            var isConfirmed = await WasRespondedToInTimeAsync(order).ConfigureAwait(false);

            return isConfirmed && IsResponseReply(order, out var reply)
                ? reply
                : null;
        }

        private Task<bool> WasRespondedToInTimeAsync(Order order)
        {
            return _responseAwaiter.GetAwaiterFor(order).HasRespondedInTimeAsync();
        }

        private bool IsResponseConfirmation(Order order, out Confirmation confirmation)
        {
            confirmation = _responseAwaiter.GetResponse(order) is Confirmation conf
                ? conf
                : null;

            return confirmation is null;
        }

        private bool IsResponseReply(Order order, out Reply reply)
        {
            reply = _responseAwaiter.GetResponse(order) is Reply rep
                ? rep
                : null;

            return reply is null;
        }

        private void OnCommandReceived(object sender, ICommand command)
        {
            if (!command.IsValid) // protocol failure
            {
                OnCommunicationErrorOccured(_commandTranslator.TranslateToErrorMessage(command));
                Debug.Write($"command {command.ID} contained protocol errors ({command.ErrorState})");
            }

            if (command.IsConfirmation())
                _responseAwaiter.AddResponse(_commandTranslator.TranslateToConfirmation(command));

            if (command.IsReply())
                _responseAwaiter.AddResponse(_commandTranslator.TranslateToReply(command));
            else
                OnOrderReceived(_commandTranslator.TranslateToOrder(command));
        }

        protected virtual void OnOrderReceived(Order order) =>
            OrderReceived?.Invoke(this, order);

        protected virtual void OnCommunicationErrorOccured(ErrorMessage errorMessage) =>
            CommunicationErrorOccured?.Invoke(this, errorMessage);
    }
}