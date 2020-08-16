using System;
using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class Reply : IResponseMessage
    {
        private readonly string _replyTo;
        private readonly Timestamp _timestamp;
        private readonly string _data;

        public static Reply Create(string replyTo, Timestamp timestamp, string data = "")
        {
            return new Reply(replyTo, timestamp, data);
        }

        public static Reply Create(string replyTo, string data = "")
        {
            return new Reply(replyTo, Timestamp.Now, data);
        }

        public Reply(string replyTo, Timestamp timestamp, string data = "")
        {
            _replyTo = replyTo ?? throw new ArgumentNullException(nameof(replyTo), $"{nameof(Reply)} has to have a target (something to reply to).");
            _timestamp = timestamp ?? throw new ArgumentNullException(nameof(timestamp), "Missing Timestamp.");
            _data = data ?? string.Empty;
        }

        public string Target => _replyTo;

        public string ReplysTo => _replyTo;
        public string Data => _data;
        public Timestamp Timestamp => _timestamp;
    }
}