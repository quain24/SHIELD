using Shield.Messaging.Commands;
using Shield.Messaging.Protocol.DataPacks;
using Shield.Timestamps;
using System;

namespace Shield.Messaging.Protocol
{
    public class OrderFactory
    {
        private readonly IIdGenerator _idGenerator;

        public OrderFactory(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator), "IDGenerator is required.");
        }

        public Order Create(string order, string target, IDataPack dataPack)
        {
            return Create(_idGenerator.GetNewID(), order, target, Timestamp.Now, dataPack);
        }

        public Order Create(string order, string target, Timestamp timestamp, IDataPack dataPack)
        {
            return Create(_idGenerator.GetNewID(), order, target, timestamp, dataPack);
        }

        public Order Create(string id, string order, string target, IDataPack dataPack)
        {
            return Create(id, order, target, Timestamp.Now, dataPack);
        }

        public Order Create(string id, string order, string target, Timestamp timestamp, IDataPack dataPack)
        {
            return new Order(id, order, target, timestamp, dataPack);
        }

        public Order Renew(Order order)
        {
            return Create(order.ExactOrder, order.Target, order.Data);
        }
        
        public Order Renew(Order order, IDataPack dataPack)
        {
            return Create(order.ExactOrder, order.Target, dataPack);
        }
    }
}