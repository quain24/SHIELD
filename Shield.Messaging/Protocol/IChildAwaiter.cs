using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    public interface IChildAwaiter
    {
        Task<bool> HasRespondedInTimeAsync();
    }
}