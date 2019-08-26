namespace Shield.HardwareCom.Models
{
    public interface IMessage
    {
        void Add(Command command);
        bool Remove(int id);
        bool Remove(Command command);
    }
}