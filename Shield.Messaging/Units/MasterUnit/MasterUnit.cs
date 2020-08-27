using System.Collections.Generic;
using System.Threading.Tasks;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Protocol;
using Shield.Messaging.Units.SlaveUnits;

namespace Shield.Messaging.Units.MasterUnit
{
    public class MasterUnit
    {
        private readonly DeviceHandlerContext _devicehandler;
        private readonly IDictionary<string, Order> _orderMap;
        private readonly Dictionary<string, IUnit> _attachedSlaveUnits = new Dictionary<string, IUnit>();

        public MasterUnit(DeviceHandlerContext devicehandler, IDictionary<string, Order> orderMap)
        {
            _devicehandler = devicehandler;
            _orderMap = orderMap;
        }

        public void Open() => _devicehandler.Open();

        public void Close() => _devicehandler.Close();

        public async Task<IDictionary<string, IUnit>> ReportAttachedSlaveUnits()
        {
            return null; // tmp
        }

        private async Task UpdateSlaveUnitDictionary(Order order)
        {
        }
    }
}