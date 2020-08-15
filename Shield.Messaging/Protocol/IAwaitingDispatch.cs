using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public interface IAwaitingDispatch
    {
        Task<bool> WasConfirmedInTimeAsync(Order order);
        Task<bool> WasRepliedToInTimeAsync(Order order);
    }
}