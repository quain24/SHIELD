using Shield.Messaging.Commands.Parts.CommandPartValidators;
using System;

namespace Shield.Messaging.Commands.Parts
{
    public class PartFactoryAutofacAdapter
    {
        private readonly Func<string, IPartValidator, IPart> _factory;
        private readonly IPartValidator _validator;

        public PartFactoryAutofacAdapter(Func<string, IPartValidator, IPart> factory, IPartValidator validator)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory), $"{nameof(factory)} cannot be NULL"); ;
            _validator = validator ?? throw new ArgumentNullException(nameof(validator), $"{nameof(validator)} cannot be NULL");
        }

        public IPart GetPart(string data) => _factory(data, _validator);
    }
}