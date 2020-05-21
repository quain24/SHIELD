namespace Shield.CommandPartValidators.RawData
{
    public interface IPartValidator
    {
        bool Validate(string data);
    }
}