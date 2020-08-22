using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public interface IAwaitingDispatch
    {
        Task<bool> WasConfirmedInTimeAsync(IConfirmable message);
        Task<bool> WasRepliedToInTimeAsync(Order order);
    }
}