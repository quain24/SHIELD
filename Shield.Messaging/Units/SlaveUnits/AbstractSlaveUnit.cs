using Shield.Messaging.Protocol;
using System;
using System.Threading.Tasks;
using Shield.Messaging.Protocol.DataPacks;

namespace Shield.Messaging.Units.SlaveUnits
{
    public abstract class AbstractSlaveUnit
    {
        private readonly ProtocolHandler _handler;

        protected AbstractSlaveUnit(string id, ProtocolHandler handler)
        {
            _handler = handler;
            ID = string.IsNullOrWhiteSpace(id) ? throw new ArgumentNullException(nameof(id), "ID has to be provided") : id;
        }

        public string ID { get; }
        public string Name { get; set; }
        public bool IsConnected { get; protected set; }

        public abstract bool CanHandle(Order order);

        public abstract Task HandleIncomingOrderAsync(Order order);

        public virtual Task<bool> SendAsync(Order order)
        {
            return _handler.SendAsync(order);
        }

        public virtual async Task<Confirmation> GetConfirmationOf(IConfirmable message)
        {
            await _handler.Order().WasConfirmedInTimeAsync(message).ConfigureAwait(false);
            return _handler.Retrieve().ConfirmationOf(message);
        }

        public virtual Task<bool> ReplyTo(Order order, IDataPack replyDataPack)
        {
            return _handler.ReplyTo(order, replyDataPack);
        }
    }
}