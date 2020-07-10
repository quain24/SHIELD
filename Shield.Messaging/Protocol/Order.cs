namespace Shield.Messaging.Protocol
{
    public class Order
    {
        private readonly string _order;
        private readonly string _target;
        private readonly string _data;

        public Order(string order, string target, string data = "")
        {
            _order = order;
            _target = target;
            _data = data;
        }

        public string Target => _target;
        public string ExactOrder => _order;
        public string Data => _data;
    }
}