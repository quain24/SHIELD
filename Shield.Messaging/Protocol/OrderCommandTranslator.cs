using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public Order Translate(ICommand command)
        {
            return new Order(command.Order.ToString(), command.Target.ToString(), command.Data.ToString());
        }
    }
}
