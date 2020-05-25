namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public interface IPartValidator
    {
        bool Validate(string data);
    }
}