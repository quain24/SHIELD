namespace Shield.Messaging.Modules.PartValidators
{
    public interface IModuleInfo
    {
        string Name { get; }

        bool ContainsOrder(string orderType);
    }
}