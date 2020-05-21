using Shield.CommandPartValidators.RawData;
using System;
using System.Collections.Generic;

namespace Shield.Messaging.RawData
{
    public abstract class Part : IPart
    {
        private readonly IPartValidator _validator;
        private bool _isValid;

        protected Part(string data, IPartValidator validator)
        {
            Data = data;
            _validator = validator;
            _ValidateData = Validate;
        }

        private Func<bool> _ValidateData;
        public virtual string Data { get; }
        public bool IsValid => _ValidateData();

        private bool Validate()
        {
            // Validation is done once per object, as object is supposed to be immutable
            _ValidateData = () => _isValid;
            return _isValid = _validator.Validate(Data);
        }

        public override bool Equals(object obj)
        {
            return obj is Part part &&
                   Data == part.Data;
        }

        public override int GetHashCode()
        {
            return -301143667 + EqualityComparer<string>.Default.GetHashCode(Data);
        }
    }
}