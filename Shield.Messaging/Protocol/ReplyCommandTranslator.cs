using Shield.GlobalConfig;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Command = Shield.Enums.Command;

namespace Shield.Messaging.Protocol
{
    public class ReplyCommandTranslator
    {
        private readonly IPartFactory _partFactory;
        private readonly CommandFactory _commandFactory;

        public ReplyCommandTranslator(IPartFactory partFactory, CommandFactory commandFactory)
        {
            _partFactory = partFactory;
            _commandFactory = commandFactory;
        }

        public ICommand Translate(Reply reply)
        {
            return _commandFactory.Create(
                _partFactory.GetPart(Command.PartType.Target, DefaultTargets.ReplyTarget),
                _partFactory.GetPart(Command.PartType.Order, reply.ReplysTo),
                _partFactory.GetPart(Command.PartType.Data, reply.Data));
        }

        public Reply Translate(ICommand command)
        {
            return new Reply(command.Order.ToString(), command.Data.ToString().Trim());
        }
    }
}