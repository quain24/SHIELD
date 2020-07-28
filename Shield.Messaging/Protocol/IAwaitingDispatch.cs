using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public interface IAwaitingDispatch
    {
        Task<bool> ConfirmedInTimeAsync(Order order);
        Task<bool> RepliedToInTimeAsync(Order order);
    }
}