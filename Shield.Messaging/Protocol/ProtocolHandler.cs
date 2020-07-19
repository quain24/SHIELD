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
        private readonly ConfirmationFactory _confirmationFactory;
        private readonly CommandTranslator _commandTranslator;
        private readonly ResponseAwaiter _responseAwaiter;

        public ProtocolHandler(DeviceHandlerContext deviceHandler, ConfirmationFactory confirmationFactory, CommandTranslator commandTranslator, ResponseAwaiter responseAwaiter)
        {
            _deviceHandler = deviceHandler;
            _confirmationFactory = confirmationFactory;
            _commandTranslator = commandTranslator;
            _responseAwaiter = responseAwaiter;
            _deviceHandler.CommandReceived += OnCommandReceived;
        }

        private event EventHandler<ICommand> CommandReceived;

        public Task<bool> SendAsync(Order order)
        {
            return _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(order));
        }

        public async Task<Confirmation> AwaitConfirmationOfAsync(Order order)
        {
            var awaiter = _responseAwaiter.GetAwaiter(order);
            var isConfirmed = await awaiter.RespondedInTime().ConfigureAwait(false);
            if (isConfirmed && IsResponseConfirmation(order, out var confirmation))
                return confirmation;
            return null;
        }

        private bool IsResponseConfirmation(Order ofOrder, out Confirmation confirmation)
        {
            confirmation = _responseAwaiter.GetResponse(ofOrder) is Confirmation conf
                ? conf
                : null;

            return confirmation is null;
        }

        public async Task<Reply> AwaitReplyToAsync(Order order)
        {
            return null; // tmp - maybe replace with task<bool> TryAwaitReply(Order order, out Reply reply)
        }

        private async void OnCommandReceived(object sender, ICommand command)
        {
            if (!command.IsValid) // protocol failure - respond immediately
            {
                await _deviceHandler.SendAsync(_confirmationFactory.GetConfirmationFor(command)).ConfigureAwait(false);
                Debug.Write($"command {command.ID} contained protocol errors ({command.ErrorState})");
            }

            if (command.IsConfirmation())
            {
                _commandTranslator.TranslateToConfirmation(command);
            }

            if (command.IsReply())
            {
                _commandTranslator.TranslateToReply(command);
            }
            else
            {
                _commandTranslator.TranslateToOrder(command);
            }
        }
    }
}