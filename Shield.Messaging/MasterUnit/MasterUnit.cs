using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Protocol;
using Shield.Messaging.SlaveUnits;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.MasterUnit
{
    public class MasterUnit
    {
        private readonly DeviceHandlerContext _devicehandler;
        private readonly IDictionary<string, Order> _orderMap;
        private readonly Dictionary<string, ISlaveUnit> _attachedSlaveUnits = new Dictionary<string, ISlaveUnit>();

        public MasterUnit(DeviceHandlerContext devicehandler, IDictionary<string, Order> orderMap)
        {
            _devicehandler = devicehandler;
            _orderMap = orderMap;
        }

        public void Open() => _devicehandler.Open();

        public void Close() => _devicehandler.Close();

        public async Task<IDictionary<string, ISlaveUnit>> ReportAttachedSlaveUnits()
        {
            return null; // tmp
        }

        private async Task UpdateSlaveUnitDictionary(Order order)
        {
        }
    }
}