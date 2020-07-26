using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.States;
using Shield.Timestamps;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Shield.Messaging.Commands
{
    public class Command : ICommand
    {
        private Func<bool> _executeValidation;
        private List<Action> _errorStateCheckMap;
        private IPart[] _enumerableParts;

        public Command(IPart id, IPart hostID, IPart target, IPart order, IPart data, Timestamp timestamp)
        {
            _executeValidation = Validate;
            ID = id;
            HostID = hostID;
            Target = target;
            Order = order;
            Data = data;
            Timestamp = timestamp;
            ErrorState = ErrorState.Unchecked();

            SetUpStateToValidationMap();
            SetupPartCollection(ID, HostID, Target, Order, Data);
        }

        public bool IsValid => _executeValidation();

        public IPart ID { get; }
        public IPart HostID { get; }
        public IPart Target { get; }
        public IPart Order { get; }
        public IPart Data { get; }
        public Timestamp Timestamp { get; }
        public ErrorState ErrorState { get; internal set; }

        #region IEnumerable<IPart> implementation

        public IEnumerator<IPart> GetEnumerator() => (IEnumerator<IPart>) _enumerableParts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable<IPart> implementation

        private void SetUpStateToValidationMap() =>
            // ReSharper disable once ComplexConditionExpression
            _errorStateCheckMap = new List<Action>()
            {
                () => { if (!ID?.IsValid ?? true) ErrorState = ErrorState.BadID(); },
                () => { if (!HostID?.IsValid ?? true) ErrorState = ErrorState.BadHostID(); },
                () => { if (!Target?.IsValid ?? true) ErrorState = ErrorState.BadTarget(); },
                () => { if (!Order?.IsValid ?? true) ErrorState = ErrorState.BadOrder(); },
                () => { if (!Data?.IsValid ?? true) ErrorState = ErrorState.BadDataPack(); }
            };

        private void SetupPartCollection(params IPart[] parts)
        {
            _enumerableParts = new IPart[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                _enumerableParts[i] = parts[i];
        }

        private bool Validate()
        {
            _errorStateCheckMap.ForEach(a => a.Invoke());
            // Still unchecked after validation = Valid
            if (ErrorState == ErrorState.Unchecked())
                ErrorState = ErrorState.Valid();

            _executeValidation = () => ErrorState == ErrorState.Valid();
            return _executeValidation();
        }
    }
}