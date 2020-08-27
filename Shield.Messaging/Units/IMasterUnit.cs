using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units
{
    public interface IMasterUnit : IUnit
    {
        IUnit CreateUnit(string type);
        bool TryRemoveUnit(string name);
        bool ExecuteOrder(Order order, IUnit targetUnit);
        IEnumerable<IUnit> GetConnectedUnits();
    }
}
