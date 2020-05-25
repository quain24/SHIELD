using System;
using System.Collections;
using System.Collections.Generic;
using static Shield.Command;

namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public class ReadOnlyPartValidatorCollection : IReadOnlyPartValidatorCollection
    {
        private readonly IDictionary<PartType, IPartValidator> _validators;
        private readonly IPartValidator _defaultValidator;

        public ReadOnlyPartValidatorCollection(IDictionary<PartType, IPartValidator> validators, IPartValidator defaultValidator)
        {
            _validators = validators ?? new Dictionary<PartType, IPartValidator>();
            _defaultValidator = defaultValidator ?? throw new ArgumentNullException(nameof(defaultValidator), "No Default Validator instance was passed.");
        }

        public int Count => _validators.Count;

        public IPartValidator GetValidatorForOrDefault(PartType type)
        {
            if (_validators.TryGetValue(type, out IPartValidator validator))
                return validator;
            return _defaultValidator;
        }

        public IPartValidator GetDefaultValidator() => _defaultValidator;

        public IEnumerator<IPartValidator> GetEnumerator()
        {
            return _validators.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}