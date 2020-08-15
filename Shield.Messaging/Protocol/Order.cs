using System.Runtime.Remoting.Messaging;
using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class Order
    {
        private readonly string _order;
        private readonly string _target;
        private readonly string _data;
        private readonly string _id = string.Empty;
        private readonly Timestamp _timestamp;

        public static Order Create(string order, string target, Timestamp timestamp, string data = "") =>
            new Order(order, target, timestamp, data);
        public static Order Create(string order, string target, string data = "") =>
            new Order(order, target, Timestamp.Now, data);

        public Order(string order, string target, Timestamp timestamp, string data = "")
        {
            _order = order;
            _target = target;
            _data = data ?? string.Empty;
            _timestamp = timestamp;
        }

        // Constructor for incoming orders - can set ID
        internal Order(string order, string target, string id, Timestamp timestamp, string data = "") : this(order, target, timestamp, data)
        {
            _id = id;
        }

        internal string ID => _id;
        public string Target => _target;
        public string ExactOrder => _order;
        public Timestamp Timestamp => _timestamp;
        public string Data => _data;
    }
}