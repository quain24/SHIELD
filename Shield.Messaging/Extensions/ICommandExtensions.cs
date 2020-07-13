using Shield.GlobalConfig;
using Shield.Messaging.Commands;

namespace Shield.Messaging.Extensions
{
    public static class ICommandExtensions
    {
        public static bool IsConfirmation(this ICommand command) =>
            command.Target.ToString() == DefaultTargets.ConfirmationTarget;

        public static bool IsReply(this ICommand command) =>
            command.Target.ToString() == DefaultTargets.ReplyTarget;
    }
}