namespace Shield.HardwareCom.Models
{
    public interface IMessage
    {
        void AddCommand(Command command);
        bool RemoveCommand(int id);
    }
}