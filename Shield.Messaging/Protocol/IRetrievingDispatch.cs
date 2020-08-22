namespace Shield.Messaging.Protocol
{
    public interface IRetrievingDispatch
    {
        Confirmation ConfirmationOf(IConfirmable message);
        Reply ReplyTo(Order order);
    }
}