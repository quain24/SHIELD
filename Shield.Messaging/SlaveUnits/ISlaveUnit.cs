using System.Collections.Generic;

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