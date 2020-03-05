namespace Shield.Helpers
{
    public interface IIdGenerator
    {
        string GetNewID();
        bool MarkAsUsedUp(string id);
    }
}