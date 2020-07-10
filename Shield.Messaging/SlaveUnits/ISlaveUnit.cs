using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.SlaveUnits
{
    public interface ISlaveUnit
    {
        string ID { get; set; }

        string GetID();
        void SetID(string id);

        string ReportState();

        IDictionary<string, bool> GetPossibleTargets();
        bool HasOrder(string order);
    }
}
