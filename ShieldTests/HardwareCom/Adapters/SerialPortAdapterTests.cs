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

namespace ShieldTests.HardwareCom.Adapters
{  

    public class SerialPortAdapterTests
    {
        SerialPortAdapter adapter = new SerialPortAdapter(new SerialPort("COM5"), 20, new Func<ICommandModel>(() => new CommandModel()));

        [Theory]
        [InlineData(@"*0001*asdfghjklqwer*", CommandType.HandShake, @"asdfghjklqwer*")]
        [InlineData(@"*0002*asdfghjklqwe", CommandType.Confirm, @"asdfghjklqwe")]
        [InlineData(@"*0002*asdfghjklqwehfgfhdgdh", CommandType.Confirm, @"asdfghjklqwehfgfhdgdh")]
        [InlineData(@"*0002*", CommandType.Confirm, @"")]
        public void CommandTranslator_returnsValidCommandObjectGivenValidData(string rawData, CommandType expectedType, string expectedData) 
        {
            MethodInfo methodInfo = typeof(SerialPortAdapter).GetMethod("CommandTranslator", BindingFlags.NonPublic | BindingFlags.Instance);
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
            MethodInfo methodInfo = typeof(SerialPortAdapter).GetMethod("CommandTranslator", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = {rawData};
            ICommandModel aa = (ICommandModel) methodInfo.Invoke(adapter, parameters);

            Assert.Equal(aa.CommandType, expectedType);
            Assert.Equal(aa.Data, expectedData);
        }
    }
}
