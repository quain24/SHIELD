using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public interface IMessageCollection
    {
        int Count { get; }

        IMessageModel AddOrUpdate(IMessageModel message);

        bool Contains(IMessageModel message);

        bool Contains(string id);

        IMessageModel GetById(string id);

        IEnumerator<KeyValuePair<string, IMessageModel>> GetEnumerator();

        IMessageModel Remove(IMessageModel message);

        bool RemoveById(string id);

        bool TryAdd(IMessageModel message);
    }
}