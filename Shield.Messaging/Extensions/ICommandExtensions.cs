using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.GlobalConfig;
using Shield.Messaging.Commands;

namespace Shield.Messaging.Extensions
{
    public static class ICommandExtensions
    {
        public static bool IsConfirmation(this ICommand command) =>
            command.Target.ToString() == ConfirmationTarget.ConfirmationTargetString;
    }
}
