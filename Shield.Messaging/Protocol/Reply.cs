using Shield.Timestamps;
using Shield.Messaging.Protocol.DataPacks;
using System;

namespace Shield.Messaging.Protocol
{
    public class Reply : IResponseMessage, IConfirmable
    {
        private readonly IDataPack _dataPack;

        public Reply(string id, string replyTo, Timestamp timestamp, IDataPack dataPack)
        {
            ReplyTo = replyTo ?? throw new ArgumentNullException(nameof(replyTo), $"{nameof(Reply)} has to have a target (something to reply to).");
            Timestamp = timestamp ?? throw new ArgumentNullException(nameof(timestamp), "Missing Timestamp.");
            _dataPack = dataPack ?? throw new ArgumentNullException(nameof(dataPack),
                "Cannot substitute DataPack with NULL.");
            ID = id;
        }

        public string ID { get; }

        public string Target => ReplyTo;

        public string ReplyTo { get; }

        public string Data => _dataPack.GetDataInTransmittableFormat();

        public Timestamp Timestamp { get; }
    }
}