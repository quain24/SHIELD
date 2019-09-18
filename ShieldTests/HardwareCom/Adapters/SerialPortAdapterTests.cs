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
using Autofac.Core;

namespace ShieldTests.HardwareCom.Adapters
{  

    public class SerialPortAdapterTests
    {
        [Theory]
        [InlineData("*0001***************", 0)]
        [InlineData("*0001*9987*09987****", 0)]
        public void CheckRawData_GivenCorrectDataVariantReturns0(string input, int expected)
        {
            MethodInfo methodinfo = typeof(SerialPortAdapter).GetMethod("CheckRawData",
                                           BindingFlags.NonPublic | BindingFlags.Instance,
                                           null,
                                           new Type[] {typeof(string)},
                                           null);

            string result = methodinfo.Invoke(new SerialPortAdapter(new SerialPort("COM5"),
                                                                    new Func<ICommandModel>(() => new CommandModel()),
                                                                    new AppSettings(new AppSettingsModel())),
                                              new object[] {input})
                                      .ToString();

            

            Assert.Equal(expected.ToString(), result);
        }

        [Theory]
        [InlineData("****0001***************", 3)]
        [InlineData("****0002*09090909090992", 3)]
        [InlineData("****0092*090*0003*90992", 3)]
        [InlineData("12332132132132132131321", -1)]
        [InlineData("89745*908798**908987888", 13)]
        [InlineData("*9745*908798**908987888", 0)]
        [InlineData("*99*11108798**908987888", 13)]
        [InlineData("012311108798**908987888", 13)]
        public void CheckRawData_GivenWrongInputReturnIndexToCutBufferFrom(string input, int expected)
        {
            MethodInfo methodinfo = typeof(SerialPortAdapter).GetMethod("CheckRawData",
                                           BindingFlags.NonPublic | BindingFlags.Instance,
                                           null,
                                           new Type[] {typeof(string)},
                                           null);

            string result = methodinfo.Invoke(new SerialPortAdapter(new SerialPort("COM5"),
                                                                    new Func<ICommandModel>(() => new CommandModel()),
                                                                    new AppSettings(new AppSettingsModel())),
                                              new object[] {input})
                                      .ToString();

            Assert.Equal(expected.ToString(), result);


        }
    }
}