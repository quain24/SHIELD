namespace Shield.Messaging.Protocol
{
    public class Reply
    {
        private readonly string _replyTo;
        private readonly string _data;

        public Reply(string replyTo, string data = "")
        {
            _replyTo = replyTo;
            _data = data;
        }

        public string ReplysTo => _replyTo;
        public string Data => _data;
    }
}