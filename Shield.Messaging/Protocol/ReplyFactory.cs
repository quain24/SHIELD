using Shield.Messaging.Commands;
using Shield.Timestamps;
using System;
using Shield.Messaging.Protocol.DataPacks;

namespace Shield.Messaging.Protocol
{
    public class ReplyFactory
    {
        private readonly IIdGenerator _idGenerator;

        public ReplyFactory(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator), "IDGenerator is required.");
        }

        public Reply Create(string replyTo, Timestamp timestamp, IDataPack dataPack)
        {
            return Create(_idGenerator.GetNewID(), replyTo, timestamp, dataPack);
        }

        public Reply Create(Order order, IDataPack dataPack)
        {
            return Create(_idGenerator.GetNewID(), order.ID, Timestamp.Now, dataPack);
        }

        public Reply Create(string id, string replyTo, IDataPack dataPack)
        {
            return Create(id, replyTo, Timestamp.Now, dataPack);
        }

        public Reply Create(string replyTo, IDataPack dataPack)
        {
            return Create(_idGenerator.GetNewID(), replyTo, Timestamp.Now, dataPack);
        }

        public Reply Create(string id, string replyTo, Timestamp timestamp, IDataPack dataPack)
        {
            return new Reply(id, replyTo, timestamp, dataPack);
        }
    }
}