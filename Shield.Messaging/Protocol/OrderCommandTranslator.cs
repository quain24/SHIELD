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

        public OrderCommandTranslator(IPartFactory partFactory, CommandFactory commandFactory)
        {
            _partFactory = partFactory;
            _commandFactory = commandFactory;
        }

        public ICommand Translate(Order order)
        {
            return _commandFactory.Create(
                _partFactory.GetPart(Command.PartType.Target, order.Target),
                _partFactory.GetPart(Command.PartType.Order, order.ExactOrder),
                string.IsNullOrWhiteSpace(order.Data)
                    ? null
                    : _partFactory.GetPart(Command.PartType.Data, order.Data));
        }

        public Order Translate(ICommand orderCommand)
        {
            return new Order(orderCommand.Order.ToString(), orderCommand.Target.ToString(), orderCommand.Data.ToString());
        }
    }
}