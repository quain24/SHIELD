﻿using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.States;
using System;
using System.Collections.Generic;

namespace Shield.Messaging.Commands
{
    public class Command : ICommand
    {
        private Func<bool> _executeValidation;
        private List<Action> _errorStateCheckMap;

        public Command(IPart id, IPart hostID, IPart target, IPart order, IPart data)
        {
            _executeValidation = Validate;
            ID = id;
            HostID = hostID;
            Target = target;
            Order = order;
            Data = data;
            Timestamp = TimestampFactory.Timestamp;
            ErrorState = ErrorState.Unchecked();

            SetUpStateToValidationMap();
        }

        public bool IsValid => _executeValidation();

        public IPart ID { get; }
        public IPart HostID { get; }
        public IPart Target { get; }
        public IPart Order { get; }
        public IPart Data { get; }
        public Timestamp Timestamp { get; }
        public ErrorState ErrorState { get; internal set; }

        private void SetUpStateToValidationMap() =>
            _errorStateCheckMap = new List<Action>()
            {
                () => { if (!ID.IsValid) ErrorState = ErrorState.BadID(); },
                () => { if (!HostID.IsValid) ErrorState = ErrorState.BadHostID(); },
                () => { if (!Target.IsValid) ErrorState = ErrorState.BadTarget(); },
                () => { if (!Order.IsValid) ErrorState = ErrorState.BadOrder(); },
                () => { if (!Data.IsValid) ErrorState = ErrorState.BadDataPack(); }
            };

        private bool Validate()
        {
            _errorStateCheckMap.ForEach(a => a.Invoke());
            // Unchecked after validation = Valid
            if (ErrorState == ErrorState.Unchecked())
                ErrorState = ErrorState.Valid();

            _executeValidation = () => ErrorState == ErrorState.Valid();
            return _executeValidation();
        }
    }
}