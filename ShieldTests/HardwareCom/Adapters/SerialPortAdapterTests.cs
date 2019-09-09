using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.Adapters;
using Shield.CommonInterfaces;
using Shield.HardwareCom.Models;
using System.IO.Ports;
using Xunit;
using Shield.Enums;
using System.Reflection;
using Autofac.Extras.Moq;
using Shield.Data.Models;
using Shield.Data;

namespace ShieldTests.HardwareCom.Adapters
{  

    public class SerialPortAdapterTests
    {

        static IAppSettings appSettings = new AppSettings(new AppSettingsModel());
        SerialPortAdapter adapter = new SerialPortAdapter(new SerialPort("COM5"), 20, new Func<ICommandModel>(() => new CommandModel()));
        ICommandModel properCommandTypeHandhake = new CommandModel{ CommandType = CommandType.HandShake, Data = "00000000000000" };



        [Theory]
        [InlineData(@"*0001*asdfghjklqwer*", CommandType.HandShake, @"asdfghjklqwer*")]
        [InlineData(@"*0002*asdfghjklqwe", CommandType.Confirm, @"asdfghjklqwe")]
        [InlineData(@"*0002*asdfghjklqwehfgfhdgdh", CommandType.Confirm, @"asdfghjklqwehfgfhdgdh")]
        [InlineData(@"*0002*", CommandType.Confirm, @"")]
        public void CommandTranslator_returnsValidCommandObjectGivenValidData(string rawData, CommandType expectedType, string expectedData) 
        {
            MethodInfo methodInfo = typeof(SerialPortAdapter).GetMethod("CommandTranslator", 
                                    BindingFlags.NonPublic | BindingFlags.Instance,
                                    null,
                                    new Type[] { typeof(string) },
                                    null);

            object[] parameters = {rawData};
            ICommandModel aa = (ICommandModel) methodInfo.Invoke(adapter, parameters);

            Assert.Equal(aa.CommandType, expectedType);
            Assert.Equal(aa.Data, expectedData);           
        }

        [Theory]
        [InlineData(@"*0101*asdfghjklqwer*", CommandType.Error, @"")]        
        [InlineData(@"*0100*asdfghjklqwer*", CommandType.Error, @"")]
        [InlineData(@"*010**asdfghjklqwer*", CommandType.Error, @"")]
        [InlineData(@"010*asdfghjklqwer*",  CommandType.Error, @"")]
        [InlineData(@"",  CommandType.Error, @"")]
        public void CommandTranslator_returnsErrorCommandObjectGivenWrongData(string rawData, CommandType expectedType, string expectedData)
        {
            MethodInfo methodInfo = typeof(SerialPortAdapter).GetMethod("CommandTranslator", 
                                    BindingFlags.NonPublic | BindingFlags.Instance,
                                    null,
                                    new Type[] { typeof(string) },
                                    null);

            object[] parameters = {rawData};
            ICommandModel aa = (ICommandModel) methodInfo.Invoke(adapter, parameters);

            Assert.Equal(aa.CommandType, expectedType);
            Assert.Equal(aa.Data, expectedData);
        }

        [Theory]
        [InlineData(CommandType.Data, "12345678901234", "12345678901234")]
        [InlineData(CommandType.Data, "1234567890", "1234567890****")]
        public void CommandTranslator_toStringGivenProperData(CommandType givenType, string givenData, string expectedData)
        {
            MethodInfo methodInfo = typeof(SerialPortAdapter).GetMethod("CommandTranslator",
                                           BindingFlags.NonPublic | BindingFlags.Instance,
                                           null,
                                           new Type[] {typeof(ICommandModel) },
                                           null);

            ICommandModel properCommandTypeData = new CommandModel{ CommandType = givenType, Data = givenData };

            object[] parameters = {properCommandTypeData };

            string aa = (string) methodInfo.Invoke(adapter, parameters);
            string comType = givenType.ToString().PadLeft(4, '0');
            comType = '*' + comType + '*';

            Assert.Equal(aa.Substring(0,6), comType);
            Assert.Equal(aa.Substring(6), expectedData);
            }






    }
}