namespace Shield.Messaging.Commands.Parts
{
    public sealed class OrderPart : Part
    {
        internal OrderPart(string data, PartValidators.IPartValidator validator) : base(data, validator)
        {
        }
    }
}