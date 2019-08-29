namespace Shield.HardwareCom.Models
{
    public interface IMessageModel
    {
        void Add(CommandModel command);
        bool Remove(int id);
        bool Remove(CommandModel command);
    }
}