namespace Shield.Messaging.Protocol
{
    public interface IRetrievingDispatch
    {
        Confirmation ConfirmationOf(Order order);
        Reply ReplyTo(Order order);
    }
}