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
        private readonly ReplyFactory _replyFactory;

        public ReplyCommandTranslator(IPartFactory partFactory, CommandFactory commandFactory, ReplyFactory replyFactory)
        {
            _partFactory = partFactory;
            _commandFactory = commandFactory;
            _replyFactory = replyFactory;
        }

        public ICommand Translate(Reply reply)
        {
            return _commandFactory.Create(
                _partFactory.GetPart(Command.PartType.ID, reply.ID),
                _partFactory.GetPart(Command.PartType.Target, DefaultTargets.ReplyTarget),
                _partFactory.GetPart(Command.PartType.Order, reply.ReplyTo),
                string.IsNullOrEmpty(reply.Data)
                    ? _partFactory.GetPart(Command.PartType.Empty, string.Empty)
                    : _partFactory.GetPart(Command.PartType.Data, reply.Data));
        }

        public Reply Translate(ICommand replyCommand)
        {
            return _replyFactory.Create(
                replyCommand.Order.ToString(),
                replyCommand.Timestamp,
                new StringDataPack(replyCommand.Data.ToString()));
        }
    }
}