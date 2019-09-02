namespace Shield.HardwareCom.Models
{
    public interface IMessageModel
    {
        void Add(ICommandModel command);
        bool Remove(int id);
        bool Remove(ICommandModel command);
    }
}