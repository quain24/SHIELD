using Shield.Messaging.Commands;
using Shield.Messaging.Protocol.DataPacks;
using Shield.Timestamps;
using System;

namespace Shield.Messaging.Protocol
{
    public class OrderFactory
    {
        private readonly IIdGenerator _idGenerator;
        private readonly IDataPackFactory _dataPackFactory;

        public OrderFactory(IIdGenerator idGenerator, IDataPackFactory dataPackFactory)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator), "IDGenerator is required.");
            _dataPackFactory = dataPackFactory ?? throw new ArgumentNullException(nameof(dataPackFactory), "Orders cannot be created in this class without a DataPackFactory instance.");
        }

        public Order Create(string id, string order, string target, Timestamp timestamp, IDataPack dataPack)
        {
            return new Order(id, order, target, timestamp, dataPack);
        }

        public Order CreateFromData<TDataType>(string id, string order, string target, Timestamp timestamp, TDataType data)
        {
            return new Order(id, order, target, timestamp, _dataPackFactory.CreateFrom(data));
        }

        public Order Create(string order, string target, IDataPack dataPack)
        {
            return Create(_idGenerator.GetNewID(), order, target, Timestamp.Now, dataPack);
        }

        public Order CreateFromData<TDataType>(string order, string target, TDataType data)
        {
            return Create(_idGenerator.GetNewID(), order, target, Timestamp.Now, _dataPackFactory.CreateFrom(data));
        }

        public Order Create(string order, string target, Timestamp timestamp, IDataPack dataPack)
        {
            return Create(_idGenerator.GetNewID(), order, target, timestamp, dataPack);
        }

        public Order CreateFromData<TDataType>(string order, string target, Timestamp timestamp, TDataType data)
        {
            return Create(_idGenerator.GetNewID(), order, target, timestamp, _dataPackFactory.CreateFrom(data));
        }

        public Order Create(string id, string order, string target, IDataPack dataPack)
        {
            return Create(id, order, target, Timestamp.Now, dataPack);
        }

        public Order Create<TDataType>(string id, string order, string target, TDataType dataPack)
        {
            return Create(id, order, target, Timestamp.Now, _dataPackFactory.CreateFrom(dataPack));
        }

        public Order Renew(Order order)
        {
            return Create(order.ExactOrder, order.Target, order.Data);
        }

        public Order Renew(Order order, IDataPack dataPack)
        {
            return Create(order.ExactOrder, order.Target, dataPack);
        }

        public Order RenewUsingRawData<TDataType>(Order order, TDataType data)
        {
            return Create(order.ExactOrder, order.Target, _dataPackFactory.CreateFrom(data));
        }
    }
}