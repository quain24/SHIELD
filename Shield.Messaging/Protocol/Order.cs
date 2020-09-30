using Shield.Messaging.Protocol.DataPacks;
using Shield.Timestamps;
using System;

namespace Shield.Messaging.Protocol
{
    public class Order : IConfirmable
    {
        public Order(string id, string order, string target, Timestamp timestamp, IDataPack dataPack)
        {
            ExactOrder = order;
            Target = target;
            Data = dataPack ?? throw new ArgumentNullException(nameof(dataPack), "A dataPack is required, cannot replace it with null");
            Timestamp = timestamp;
            ID = id;
        }

        public string ID { get; }

        public string Target { get; }

        public string ExactOrder { get; }

        public Timestamp Timestamp { get; }

        public IDataPack Data { get; }
    }
}