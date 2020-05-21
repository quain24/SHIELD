using Shield.CommandPartValidators.RawData;

namespace Shield.Messaging.RawData.CommandPartValidators
{
    internal class AllwaysGoodValidator : IPartValidator
    {
        public bool Validate(string data) => true;
    }
}