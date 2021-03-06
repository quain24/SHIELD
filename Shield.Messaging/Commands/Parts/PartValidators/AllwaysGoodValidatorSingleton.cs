﻿using System;

namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public sealed class AllwaysGoodValidatorSingleton : IPartValidator
    {
        private static readonly Lazy<AllwaysGoodValidatorSingleton> AllwaysGoodValidator = new Lazy<AllwaysGoodValidatorSingleton>(() => new AllwaysGoodValidatorSingleton());

        private AllwaysGoodValidatorSingleton()
        {
        }

        public static AllwaysGoodValidatorSingleton Instance => AllwaysGoodValidator.Value;

        public bool Validate(string data) => true;
    }
}