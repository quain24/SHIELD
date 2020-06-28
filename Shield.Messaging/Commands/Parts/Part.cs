using Shield.Messaging.Commands.Parts.PartValidators;
using System;
using System.Collections.Generic;

namespace Shield.Messaging.Commands.Parts
{
    public abstract class Part : ValueObject, IPart
    {
        private readonly IPartValidator _validator;
        private bool _isValid;

        protected Part(string data, IPartValidator validator)
        {
            Data = data;
            _validator = validator;
            ValidateData = Validate;
        }

        private Func<bool> ValidateData;
        public virtual string Data { get; }
        public bool IsValid => ValidateData();

        private bool Validate()
        {
            // Validation is done once per object, as object is immutable
            ValidateData = () => _isValid;
            return _isValid = _validator.Validate(Data);
        }

        public override string ToString()
        {
            return Data;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Data;
        }
    }
}