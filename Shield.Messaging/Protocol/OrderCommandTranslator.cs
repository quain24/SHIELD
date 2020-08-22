using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Command = Shield.Enums.Command;
using ICommand = Shield.Messaging.Commands.ICommand;

namespace Shield.Messaging.Protocol
{
    public class OrderCommandTranslator
    {
        private readonly IPartFactory _partFactory;
        private readonly CommandFactory _commandFactory;
        private readonly OrderFactory _orderFactory;

        public OrderCommandTranslator(IPartFactory partFactory, CommandFactory commandFactory, OrderFactory orderFactory)
        {
            _partFactory = partFactory;
            _commandFactory = commandFactory;
            _orderFactory = orderFactory;
        }

        public ICommand Translate(Order order)
        {
            return _commandFactory.Create(
                _partFactory.GetPart(Command.PartType.ID, order.ID),
                _partFactory.GetPart(Command.PartType.Target, order.Target),
                _partFactory.GetPart(Command.PartType.Order, order.ExactOrder),
                string.IsNullOrEmpty(order.Data)
                    ? _partFactory.GetPart(Command.PartType.Empty, string.Empty)
                    : _partFactory.GetPart(Command.PartType.Data, order.Data));
        }

        public Order Translate(ICommand orderCommand)
        {
            return _orderFactory.Create(
                orderCommand.ID.ToString(),
                orderCommand.Order.ToString(),
                orderCommand.Target.ToString(),
                orderCommand.Timestamp,
                new StringDataPack(orderCommand.Data.ToString()));
        }
    }
}