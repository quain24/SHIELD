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

        public ProtocolHandler(DeviceHandlerContext deviceHandler, ConfirmationFactory confirmationFactory, CommandTranslator commandTranslator)
        {
            _deviceHandler = deviceHandler;
            _confirmationFactory = confirmationFactory;
            _commandTranslator = commandTranslator;
            _deviceHandler.CommandReceived += OnCommandReceived;
        }

        private event EventHandler<ICommand> CommandReceived;

        public Task<bool> SendAsync(Order order)
        {
            return _deviceHandler.SendAsync(_commandTranslator.TranslateToCommand(order));
        }

        public async Task<Confirmation> AwaitConfirmationOfAsync(Order order)
        {
            return null; // tmp
        }

        public async Task<Order> AwaitReplyToAsync(Order order)
        {
            return null; // tmp
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