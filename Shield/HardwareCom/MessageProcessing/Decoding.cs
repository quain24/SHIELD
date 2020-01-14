using Shield.Enums;
using Shield.HardwareCom.Models;
using System.Collections.Generic;
using System.Linq;

namespace Shield.HardwareCom
{
    public class Decoding : IDecoding
    {
        public Errors Check(IMessageHWComModel message)
        {
            if (message is null)
                return Errors.IsNull;

            List<ICommandModel> badOrUnknown = message
                .Where(c =>
                    c.CommandType == CommandType.Unknown ||
                    c.CommandType == CommandType.Error ||
                    c.CommandType == CommandType.Partial)
                .ToList();

            if (badOrUnknown.Any() == false)
                return Errors.None;

            Errors errors = Errors.None;

            foreach (ICommandModel c in badOrUnknown)
            {
                if (c.CommandType == CommandType.Error)
                    errors = errors | Errors.GotErrorCommands;
                else if (c.CommandType == CommandType.Unknown)
                    errors = errors | Errors.GotUnknownCommands;
                else if (c.CommandType == CommandType.Partial)
                    errors = errors | Errors.GotPartialCommands;
            }
            return errors;
        }
    }
}