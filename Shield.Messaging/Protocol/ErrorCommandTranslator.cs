﻿using Shield.Messaging.Commands;
using System.Linq;

namespace Shield.Messaging.Protocol
{
    public class ErrorCommandTranslator
    {
        public ErrorMessage Translate(ICommand command)
        {
            var data = new string[command.Count()];
            var counter = 0;

            foreach (var part in command)
            {
                data[counter] = part.GetType().Name + " | " + part.ToString();
                counter++;
            }

            return new ErrorMessage(command.ErrorState, command.Timestamp, data);
        }
    }
}