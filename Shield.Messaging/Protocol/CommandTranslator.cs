using Shield.Messaging.Commands;
using System;

namespace Shield.Messaging.Protocol
{
    public class CommandTranslator
    {
        private readonly OrderCommandTranslator _orderCommandTranslator;
        private readonly ReplyCommandTranslator _replyCommandTranslator;
        private readonly ConfirmationCommandTranslator _confirmationCommandTranslator;
        private readonly ErrorCommandTranslator _errorCommandTranslator;

        public CommandTranslator(OrderCommandTranslator orderCommandTranslator,
            ReplyCommandTranslator replyCommandTranslator,
            ConfirmationCommandTranslator confirmationCommandTranslator,
            ErrorCommandTranslator errorCommandTranslator)
        {
            _orderCommandTranslator = orderCommandTranslator;
            _replyCommandTranslator = replyCommandTranslator;
            _confirmationCommandTranslator = confirmationCommandTranslator;
            _errorCommandTranslator = errorCommandTranslator;
        }

        public Order TranslateToOrder(ICommand orderCommand) =>
            _orderCommandTranslator.Translate(orderCommand);

        public ICommand TranslateToCommand(Order order) =>
            _orderCommandTranslator.Translate(order);

        public Reply TranslateToReply(ICommand replyCommand) =>
            _replyCommandTranslator.Translate(replyCommand);

        public ICommand TranslateToCommand(Reply reply) =>
            _replyCommandTranslator.Translate(reply);

        public Confirmation TranslateToConfirmation(ICommand confirmationCommand) =>
            _confirmationCommandTranslator.Translate(confirmationCommand);

        public ICommand TranslateToCommand(Confirmation confirmation) =>
            _confirmationCommandTranslator.Translate(confirmation);

        public ErrorMessage TranslateToErrorMessage(ICommand command) =>
            _errorCommandTranslator.Translate(command);

        public ICommand TranslateToCommand(IConfirmable message)
        {
            switch (message)
            {
                case Order order:
                    return TranslateToCommand(order);

                case Reply reply:
                    return TranslateToCommand(reply);

                default:
                    throw new Exception($"Unknown base type of {nameof(IConfirmable)} passed.");
            }
        }
    }
}