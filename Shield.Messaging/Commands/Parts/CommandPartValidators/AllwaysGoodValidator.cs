using System.Threading;

namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public class AllwaysGoodValidator : IPartValidator
    {
        public bool Validate(string data) => true;
    }
}