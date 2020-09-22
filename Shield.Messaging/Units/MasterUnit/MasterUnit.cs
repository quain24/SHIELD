using System.Collections.Generic;
using System.Threading.Tasks;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Protocol;
using Shield.Messaging.Units.SlaveUnits;

namespace Shield.Messaging.Units.MasterUnit
{
    public class MasterUnit
    {
        private readonly DeviceHandlerContext _deviceHandler;
        private readonly OrderCommandTranslator _translator;
        private readonly Dictionary<string, IUnit> _attachedSlaveUnits = new Dictionary<string, IUnit>();

        public MasterUnit(DeviceHandlerContext deviceHandler, OrderCommandTranslator translator)
        {
            _deviceHandler = deviceHandler;
            _translator = translator;
        }

        public void Open() => _deviceHandler.Open();

        public void Close() => _deviceHandler.Close();

        public async Task<IDictionary<string, IUnit>> ReportAttachedSlaveUnits()
        {
            
            return null; // tmp
        }

        private async Task UpdateSlaveUnitDictionary(Order order)
        {
        }
    }
}