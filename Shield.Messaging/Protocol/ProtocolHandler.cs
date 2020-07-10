using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Extensions;

namespace Shield.Messaging.Protocol
{
    public class ProtocolHandler
    {
        private readonly DeviceHandlerContext _deviceHandler;
        private readonly OrderCommandTranslator _orderCommandTranslator;
        private readonly ConfirmationFactory _confirmationFactory;
        private readonly ConfirmationCommandTranslator _confirmationCommandTranslator;
        private readonly ReplyCommandTranslator _replyCommandTranslator;

        public ProtocolHandler(DeviceHandlerContext deviceHandler,
            OrderCommandTranslator orderCommandTranslator,
            ConfirmationFactory confirmationFactory,
            ConfirmationCommandTranslator confirmationCommandTranslator,
            ReplyCommandTranslator replyCommandTranslator)
        {
            _deviceHandler = deviceHandler;
            _orderCommandTranslator = orderCommandTranslator;
            _confirmationFactory = confirmationFactory;
            _confirmationCommandTranslator = confirmationCommandTranslator;
            _replyCommandTranslator = replyCommandTranslator;
            _deviceHandler.CommandReceived += OnCommandReceived;
        }

        private event EventHandler<ICommand> CommandReceived; 

        public Task<bool> SendAsync(Order order)
        {
            return _deviceHandler.SendAsync(_orderCommandTranslator.Translate(order));
        }

        public async Task<bool> AwaitConfirmationOfAsync(Order order)
        {

        }

        public async Task<Order> AwaitReplyToAsync(Order order)
        {

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
                _confirmationCommandTranslator.Translate(_confirmationFactory.GetConfirmationFor(command));
            }

            if (command.IsReply())
            {
                _replyCommandTranslator.Translate(command);
            }

            else
            {
                _orderCommandTranslator.Translate(command);
            }

        }
    }
}
