using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Shield.HardwareCom.Adapters;
using Shield.HardwareCom.Factories;
using Shield.Enums;

namespace Shield.HardwareCom
{
    public class HardwareComAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterAssemblyTypes(Assembly.Load($"{nameof(Shield)}.{nameof(HardwareCom)}"))
            //       .Except<ComPortFactory>(cpf => cpf.As<IComPortFactory>().SingleInstance())
            //       .Except<CommunicationDeviceFactory>(cdf => cdf.As<ICommunicationDeviceFactory>().SingleInstance());
                   //.Except<SerialPortAdapter>(spa => spa.As<ICommunicationDevice>()
                   //                                     .Keyed<ICommunicationDevice>(DeviceType.serial)
                   //                                     .WithParameter(new ResolvedParameter(                                                                 
                   //                                                   (pi, ctx) => pi.ParameterType == typeof(SerialPort) && pi.Name == "port",
                   //                                                   (pi, ctx) => new SerialPort())))
                   //.Except<MoqAdapter>(moq => moq.As<ICommunicationDevice>()
                   //                              .Keyed<ICommunicationDevice>(DeviceType.moq))
                   
                                      
                //.Except<ComSender>(cs => cs.As<IComSender>()
                //    .WithParameter(
                //        new ResolvedParameter(
                //            (pi, ctx) => pi.ParameterType == typeof(SerialPort) && pi.Name == "port",
                //            (pi, ctx) => new SerialPort()
                //            )
                //        )
                //    )
                //.AsImplementedInterfaces()
                //.InstancePerDependency();


            // Models registration (single interface per model)
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.Name.EndsWith("Model"))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // Factories registration (single interface per factory) both normal and autofac's factories
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Except<CommunicationDeviceFactory>(icdf => icdf.As<ICommunicationDeviceFactory>().SingleInstance())
                   .Where(t => t.Name.EndsWith("Factory"))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            
            // tymczasowo do wszystkiego innego
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Except<IMessanger>()
                   .Except<Messanger>()
                   .Except<MoqAdapter>()
                   .Except<SerialPortAdapter>()
                   .AsImplementedInterfaces()
                   .InstancePerDependency();
            
            base.Load(builder);
        }
    }
}
