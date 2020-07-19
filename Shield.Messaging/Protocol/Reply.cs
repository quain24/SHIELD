using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class Reply : IResponseMessage
    {
        private readonly string _replyTo;
        private readonly Timestamp _timestamp;
        private readonly string _data;

        public Reply(string replyTo, Timestamp timestamp, string data = "")
        {
            _replyTo = replyTo;
            _timestamp = timestamp;
            _data = data;
        }

        public string Target => _replyTo;

        public string ReplysTo => _replyTo;
        public string Data => _data;
        public Timestamp Timestamp => _timestamp;
    }
}