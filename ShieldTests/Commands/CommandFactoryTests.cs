using System.Collections.Generic;
using System.Diagnostics;
using Shield;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;

namespace ShieldTests.Commands
{
    public class CommandFactoryTests
    {
        private PartFactoryAutofacAdapter idfac;
        private PartFactoryAutofacAdapter hostidfac;
        private PartFactoryAutofacAdapter typefac;
        private PartFactoryAutofacAdapter dataidfac;
        private PartFactoryAutofacAdapter emptyfac;

        private readonly HighPrecisionClock clock = new HighPrecisionClock();


        public CommandFactoryTests()
        {
            //CommandFactory(char separator, IPartFactory factory, CommandFactoryAutoFacAdapter commandFactory)
            var v1 = new OnlyAlphanumericAllowedValidator();
            var v2 = AllwaysGoodValidatorSingleton.Instance;

            Debug.WriteLine(clock.UtcNow + " " + clock.UtcNow.Ticks);
            bool ttt;
            bool tttt;
            for (int i = 0 ; i < 50000 ; i++)
            {
                ttt = v1.Validate("abc123ABC ");
                tttt = v1.Validate("abc%");
            }
            Debug.WriteLine(clock.UtcNow + " " + clock.UtcNow.Ticks);

            var a1 = idfac = new PartFactoryAutofacAdapter((data, validator) => new IDPart(data, validator), v1);
            var a2 = hostidfac = new PartFactoryAutofacAdapter((data, validator) => new HostIDPart(data, validator), v1);
            var a3 = typefac = new PartFactoryAutofacAdapter((data, validator) => new IDPart(data, validator), v1);
            var a4 = dataidfac = new PartFactoryAutofacAdapter((data, validator) => new DataPart(data, validator), v1);
            var a5 = emptyfac = new PartFactoryAutofacAdapter((data, validator) => new EmptyPart(validator), v2);

            



            var partFactory = new PrecisePartFactory(new Dictionary<Shield.Enums.Command.PartType, PartFactoryAutofacAdapter>()
            {
                [Shield.Enums.Command.PartType.ID] = a1,
                [Shield.Enums.Command.PartType.HostID] = a2,
                [Shield.Enums.Command.PartType.Type] = a3,
                [Shield.Enums.Command.PartType.Data] = a4,
                [Shield.Enums.Command.PartType.Empty] = a5
            });
        }
    }
}