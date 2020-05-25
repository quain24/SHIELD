namespace Shield.Messaging.Commands.Parts
{
    public interface IPart
    {
        string Data { get; }
        bool IsValid { get; }
    }
}