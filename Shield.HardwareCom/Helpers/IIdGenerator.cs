using System.Collections.Generic;

namespace Shield.HardwareCom.Helpers
{
    public interface IIdGenerator
    {
        void FlushUsedUpIdsBuffer();

        string GetNewID();

        IEnumerable<string> GetUsedUpIds();
        
        void MarkAsUsedUp(params string[] ids);
    }
}