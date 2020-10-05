using System;
using Shield.Messaging.Protocol;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.Units.MasterUnit
{
    public class MasterUnit : IDisposable
    {
        private readonly Dictionary<string, IUnit> _attachedSlaveUnits = new Dictionary<string, IUnit>();
        private readonly ProtocolHandler _handler;

        public MasterUnit(ProtocolHandler handler)
        {
            _handler = handler;
            _handler.AddOrderReceivedHandler(HandleIncomingOrder);
        }

        private Task HandleIncomingOrder(Order order)
        {
            return Task.CompletedTask;
        }

        public async Task<IDictionary<string, IUnit>> ReportAttachedSlaveUnits()
        {
            return null; // tmp
        }

        private async Task UpdateSlaveUnitDictionary(Order order)
        {
        }

        public void Dispose()
        {
            _handler?.RemoveOrderReceivedHandler(HandleIncomingOrder);
        }
    }
}