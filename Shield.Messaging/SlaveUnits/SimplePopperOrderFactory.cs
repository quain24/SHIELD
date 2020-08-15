using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class SimplePopperOrderFactory
    {
        public Order FlashRed(string target, int milliseconds)
        {
            return new Order("fr", target, Timestamp.Now, milliseconds.ToString());
        }

        public Order FlashGreen(string target, int milliseconds)
        {
            return new Order("gr", target, Timestamp.Now, milliseconds.ToString());
        }

        public Order GetNewOrder(string order, string target)
        {
            return new Order(order, target, Timestamp.Now, null);
        }
    }
}