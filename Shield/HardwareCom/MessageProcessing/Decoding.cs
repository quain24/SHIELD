using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public static class Decoding
    {
        public static MessageErrors Check(IMessageModel message)
        {
            if (message is null)
                return MessageErrors.IsNull;

            List<ICommandModel> badOrUnknown = message
                .Where(c =>
                    c.CommandType == CommandType.Unknown ||
                    c.CommandType == CommandType.Error ||
                    c.CommandType == CommandType.Partial)
                .ToList();

            if (badOrUnknown.Any() == false)
                return MessageErrors.None;

            MessageErrors errors = MessageErrors.None;

            foreach (ICommandModel c in badOrUnknown)
            {
                if (c.CommandType == CommandType.Error)
                    errors = errors | MessageErrors.GotErrorCommands;
                else if (c.CommandType == CommandType.Unknown)
                    errors = errors | MessageErrors.GotUnknownCommands;
                else if (c.CommandType == CommandType.Partial)
                    errors = errors | MessageErrors.GotPartialCommands;
            }
            return errors;
        }
    }
}
