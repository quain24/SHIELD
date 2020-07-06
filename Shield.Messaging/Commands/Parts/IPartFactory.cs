
namespace Shield.Messaging.Commands.Parts
{
    public interface IPartFactory
    {
        IPart GetPart(Enums.Command.PartType type, string data);
    }
}