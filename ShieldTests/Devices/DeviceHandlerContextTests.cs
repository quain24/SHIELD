using Xunit;
using Shield.Messaging.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.Adapters;
using Shield.COMDevice;
using Shield.CommonInterfaces;
using System.IO.Ports;
using ShieldTests.RawData;
using Shield.Messaging.RawData;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Shield.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;
using static Shield.Enums.Command;
using Xunit.Abstractions;

namespace Shield.Messaging.Devices.Tests
{
    public class DeviceHandlerContextTests
    {
        public ITestOutputHelper Output;

        private PartFactoryAutofacAdapter idfac;
        private PartFactoryAutofacAdapter hostidfac;
        private PartFactoryAutofacAdapter targetfac;
        private PartFactoryAutofacAdapter orderfac;
        private PartFactoryAutofacAdapter datafac;
        private PartFactoryAutofacAdapter emptyfac;

        private readonly HighPrecisionClock clock = new HighPrecisionClock();
        public DeviceHandlerContextTests(ITestOutputHelper output)
        {
            Output = output;

            ICommunicationDeviceSettings settings = new SerialPortSettings()
            {
                BaudRate = /*19200*/921600,
                Encoding = 20127,
                PortNumber = 4,
                WriteTimeout = -1,                
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = -1,
            };

            var dataSplitter = new StandardDataStreamSplitter('#');

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

            var comfacadap = new CommandFactoryAutoFacAdapter((id, hostid, target, order, data) => new Command(id, hostid, target, order, data));

            var Factory = new CommandFactory('*', partFactory, comfacadap);

            ICommunicationDeviceAsync com = new Shield.COMDevice.SerialPortAdapter(settings);


            DHC = new DeviceHandlerContext(com, dataSplitter, Factory);

        }

        public DeviceHandlerContext DHC;

        [Fact()]
        public async Task ListenAsyncTest()
        {
            int i = 0;
            DHC.Open();
            DHC.StartListeningAsync();
            while (i < 1000)
            {
                i++;
                await Task.Delay(1).ConfigureAwait(false);
                if(i % 100 == 0)
                {
                    DHC.Close();
                    DHC.Open();
                    DHC.StartListeningAsync();
                }
                //Console.WriteLine(DHC.list.Count);
                
            }
            DHC.StopListeningAsync();
            //DHC.Close();
            Assert.True(1 == 1);
        }
    }
}