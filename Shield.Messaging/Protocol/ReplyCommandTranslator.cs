using System;
using System.Linq;
using Shield.Messaging.Commands;

namespace Shield.Messaging.Protocol
{
    public class ReplyCommandTranslator
    {
        public Reply Translate(ICommand command)
        {
            return new Reply(command.Order.ToString(), command.Data.ToString().Trim());
        }
    }
}