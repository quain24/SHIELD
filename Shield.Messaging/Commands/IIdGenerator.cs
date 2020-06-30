using System.Collections.Generic;

namespace Shield.Messaging.Commands
{
    public interface IIdGenerator
    {
        void FlushUsedUpIdsBuffer();

        string GetNewID();

        IEnumerable<string> GetUsedUpIds();
        
        void MarkAsUsedUp(params string[] ids);
    }
}