using Shield;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;
using System.Collections.Generic;
using System.Diagnostics;
using Shield.Timestamps;
using Xunit;
using static Shield.Enums.Command;

namespace ShieldTests.Commands
{
    public class CommandFactory2Tests
    {
        private PartFactoryAutofacAdapter idfac;
        private PartFactoryAutofacAdapter hostidfac;
        private PartFactoryAutofacAdapter targetfac;
        private PartFactoryAutofacAdapter orderfac;
        private PartFactoryAutofacAdapter datafac;
        private PartFactoryAutofacAdapter emptyfac;

        private readonly HighPrecisionClock clock = new HighPrecisionClock();

        public CommandFactory2Tests()
        {
            //CommandFactory(char separator, IPartFactory factory, CommandFactoryAutoFacAdapter commandFactory)
            var v1 = new OnlyAlphanumericAllowedValidator();
            var v2 = AllwaysGoodValidatorSingleton.Instance;

            var a1 = idfac = new PartFactoryAutofacAdapter((data, validator) => new IDPart(data, validator), v1);
            var a2 = hostidfac = new PartFactoryAutofacAdapter((data, validator) => new HostIDPart(data, validator), v1);
            var a3 = targetfac = new PartFactoryAutofacAdapter((data, validator) => new TargetPart(data, validator), v1);
            var a4 = orderfac = new PartFactoryAutofacAdapter((data, validator) => new OrderPart(data, validator), v1);
            var a5 = datafac = new PartFactoryAutofacAdapter((data, validator) => new DataPart(data, validator), v1);
            var a6 = emptyfac = new PartFactoryAutofacAdapter((data, validator) => new EmptyPart(validator), v2);

            var partFactory = new PrecisePartFactory(new Dictionary<PartType, PartFactoryAutofacAdapter>()
            {
                [PartType.ID] = a1,
                [PartType.HostID] = a2,
                [PartType.Target] = a3,
                [PartType.Order] = a4,
                [PartType.Data] = a5,
                [PartType.Empty] = a6
            });

            var comfacadap = new CommandFactoryAutoFacAdapter((id, hostid, target, order, data, Timestamp) => new Command(id, hostid, target, order, data, TimestampFactory.Timestamp));

            Factory = new CommandFactory('*', partFactory, comfacadap, new IdGenerator(4));
        }

        public CommandFactory Factory;

        [Fact]
        public void NewFactoryTestCommon()
        {
            var command = Factory.TranslateFrom(new Shield.Messaging.RawData.RawCommand("0123*abcd*mod1*order1"));
            var command2 = Factory.TranslateFrom(new Shield.Messaging.RawData.RawCommand("01231*abcdd*mod1*order1*0123456789"));
            var list = new List<ICommand>();

            var timeout = new Timeout(1);

            var t1 = TimestampFactory.Timestamp;
            Debug.WriteLine(clock.UtcNow + "  " + clock.UtcNow.Ticks);
            bool val;
            for (int i = 0; i < 5000000; i++)
            {
                command = Factory.TranslateFrom(new Shield.Messaging.RawData.RawCommand("01231*abcdd*mod1*order1*0123456789"));
                val = command.IsValid;
                val = command.IsValid;
                //val = command.IsValid;
            }
            Debug.WriteLine(clock.UtcNow + "  " + clock.UtcNow.Ticks);
            var t2 = TimestampFactory.Timestamp;

            Debug.WriteLine(timeout.IsExceeded(t1));
            Debug.WriteLine(t2.Difference(t1));
            Debug.WriteLine(timeout.ToString());

            Debug.WriteLine(clock.UtcNow + "  " + clock.UtcNow.Ticks);
            for (int i = 0; i < 50000; i++)
            {
                list.Add(Factory.TranslateFrom(new Shield.Messaging.RawData.RawCommand("0123*abcd*mod1*order1*0123456789")));
                list.Add(Factory.TranslateFrom(new Shield.Messaging.RawData.RawCommand("01231*abcdd*mod1*order1*0123456789")));
            }
            Debug.WriteLine(clock.UtcNow + "  " + clock.UtcNow.Ticks);
            Debug.WriteLine(clock.UtcNow + "  " + clock.UtcNow.Ticks);
        }

        // TODO validation needs overhaul, every part.
        // Bad data pack to only be marked if target order requires data pack
        // Think how to DI all of this
    }
}