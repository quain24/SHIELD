using Shield.Messaging.Commands;

namespace Shield.Messaging.Protocol
{
    public class CommandTranslator
    {
        private readonly OrderCommandTranslator _orderCommandTranslator;
        private readonly ReplyCommandTranslator _replyCommandTranslator;
        private readonly ConfirmationCommandTranslator _confirmationCommandTranslator;

        public CommandTranslator(OrderCommandTranslator orderCommandTranslator, ReplyCommandTranslator replyCommandTranslator, ConfirmationCommandTranslator confirmationCommandTranslator)
        {
            _orderCommandTranslator = orderCommandTranslator;
            _replyCommandTranslator = replyCommandTranslator;
            _confirmationCommandTranslator = confirmationCommandTranslator;
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
    }
}