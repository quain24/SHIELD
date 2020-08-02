using Shield.Enums;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;
using System.Collections.Generic;

namespace ShieldTests.Messaging.Commands.Parts
{
    internal class PartFactoryTestObjects
    {
        public static IPartFactory GetAlwaysValidPartFactory()
        {
            var v1 = AllwaysGoodValidatorSingleton.Instance;

            var a1 = new PartFactoryAutofacAdapter((data, validator) => new IDPart(data, validator), v1);
            var a2 = new PartFactoryAutofacAdapter((data, validator) => new HostIDPart(data, validator), v1);
            var a3 = new PartFactoryAutofacAdapter((data, validator) => new TargetPart(data, validator), v1);
            var a4 = new PartFactoryAutofacAdapter((data, validator) => new OrderPart(data, validator), v1);
            var a5 = new PartFactoryAutofacAdapter((data, validator) => new DataPart(data, validator), v1);
            var a6 = new PartFactoryAutofacAdapter((data, validator) => new EmptyPart(validator), v1);

            var partFactory = new PrecisePartFactory(new Dictionary<Command.PartType, PartFactoryAutofacAdapter>()
            {
                [Command.PartType.ID] = a1,
                [Command.PartType.HostID] = a2,
                [Command.PartType.Target] = a3,
                [Command.PartType.Order] = a4,
                [Command.PartType.Data] = a5,
                [Command.PartType.Empty] = a6
            });

            return partFactory;
        }

        public static IPartFactory getAllwaysInvalidPartFactory()
        {
            var v1 = AllwaysBadValidatorSingleton.Instance;

            var a1 = new PartFactoryAutofacAdapter((data, validator) => new IDPart(data, validator), v1);
            var a2 = new PartFactoryAutofacAdapter((data, validator) => new HostIDPart(data, validator), v1);
            var a3 = new PartFactoryAutofacAdapter((data, validator) => new TargetPart(data, validator), v1);
            var a4 = new PartFactoryAutofacAdapter((data, validator) => new OrderPart(data, validator), v1);
            var a5 = new PartFactoryAutofacAdapter((data, validator) => new DataPart(data, validator), v1);
            var a6 = new PartFactoryAutofacAdapter((data, validator) => new EmptyPart(validator), v1);

            var partFactory = new PrecisePartFactory(new Dictionary<Command.PartType, PartFactoryAutofacAdapter>()
            {
                [Command.PartType.ID] = a1,
                [Command.PartType.HostID] = a2,
                [Command.PartType.Target] = a3,
                [Command.PartType.Order] = a4,
                [Command.PartType.Data] = a5,
                [Command.PartType.Empty] = a6
            });

            return partFactory;
        }
    }
}