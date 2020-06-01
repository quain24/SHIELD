using Xunit;
using Shield.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;
using System.Net.Http.Headers;
using Shield.Commands.Parts;
using System.Diagnostics;

namespace Shield.Messaging.Commands.Tests
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
            var v1 = new AllAlphanumericAllowedValidator(4);
            var v2 = new AllwaysGoodValidator();
            var v3 = new DataPartValidator(10, '*', '#');
            var v4 = new TypePartValidator(new KnownCommandTypes(new List<string>(){"Hello", "GetMe", "TryMe", "Data" }));


            var a1 = idfac = new PartFactoryAutofacAdapter((data, validator) => new IDPart(data, validator), v1);
            var a2 = hostidfac = new PartFactoryAutofacAdapter((data, validator) => new HostIDPart(data, validator), v1);
            var a3 = typefac = new PartFactoryAutofacAdapter((data, validator) => new TypePart(data, validator), v4);
            var a4 = dataidfac = new PartFactoryAutofacAdapter((data, validator) => new DataPart(data, validator), v3);
            var a5 = emptyfac = new PartFactoryAutofacAdapter((data, validator) => new EmptyPart(validator), v2);

            



            var partFactory = new PrecisePartFactory(new Dictionary<Enums.Command.PartType, PartFactoryAutofacAdapter>()
            {
                [Enums.Command.PartType.ID] = a1,
                [Enums.Command.PartType.HostID] = a2,
                [Enums.Command.PartType.Type] = a3,
                [Enums.Command.PartType.Data] = a4,
                [Enums.Command.PartType.Empty] = a5
            });

            var comfacadap = new CommandFactoryAutoFacAdapter((id, hostid, type, data) => new Command(id, hostid, type, data));

            Factory = new CommandFactory('*', partFactory, comfacadap);
        }

        public CommandFactory Factory;
        

        [Fact()]
        public void CommandFactoryTest()
        {
            var command = Factory.TranslateFrom(new RawData.RawCommand("0123*abcd*Hello*1234567890"));
            
            Debug.WriteLine(clock.UtcNow);
            var list = new List<ICommand>();
            for(int i = 0 ; i < 100000 ; i++)
            {
                list.Add(Factory.TranslateFrom(new RawData.RawCommand("0123*abcd*Hello*1234567890")));
            }
            Debug.WriteLine(clock.UtcNow);

            var teste = Factory.TranslateFrom(new RawData.RawCommand("912313131312312312"));


            var knownCommandTypes = new KnownCommandTypes(new List<string>() { "aa", "bb", " c"});
            foreach(var entry in knownCommandTypes)
                Debug.WriteLine(entry);

            var expected = new Command(idfac.GetPart("0123"), hostidfac.GetPart("abcd"), typefac.GetPart("Hello"), dataidfac.GetPart("1234567890"));
            Assert.True(command.ID.Data == expected.ID.Data);
        }

        [Fact()]
        public void TranslateFromTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}