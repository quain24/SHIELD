namespace Shield.Messaging.Protocol
{
    public interface IRetrievingDispatch
    {
        Confirmation RetrieveConfirmationOf(Order order);
        Reply RetrieveReplyTo(Order order);
    }
}