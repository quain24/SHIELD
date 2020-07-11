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

        public Order TranslateToOrder(ICommand command) =>
            _orderCommandTranslator.Translate(command);

        public ICommand TranslateToCommand(Order order) =>
            _orderCommandTranslator.Translate(order);

        public Reply TranslateToReply(ICommand command) =>
            _replyCommandTranslator.Translate(command);

        public ICommand TranslateToCommand(Reply reply) =>
            _replyCommandTranslator.Translate(reply);

        public Confirmation TranslateToConfirmation(ICommand command) =>
            _confirmationCommandTranslator.Translate(command);

        public ICommand TranslateToCommand(Confirmation confirmation) =>
            _confirmationCommandTranslator.Translate(confirmation);
    }
}