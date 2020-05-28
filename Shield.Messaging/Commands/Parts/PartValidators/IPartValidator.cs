namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public interface IPartValidator
    {
        bool Validate(string data);
    }
}