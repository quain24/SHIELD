using Shield.Messaging.Modules.PartValidators;

namespace Shield.Messaging.Modules
{
    public interface IModuleInfoCollection
    {
        bool Contains(string name);
        IModuleInfo GetByName(string name);
    }
}