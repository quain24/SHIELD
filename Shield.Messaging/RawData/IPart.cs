namespace Shield.Messaging.RawData
{
    public interface IPart
    {
        string Data { get; }
        bool IsValid { get; }

        bool Equals(object obj);
        int GetHashCode();
    }
}