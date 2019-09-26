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
using Shield.CommonInterfaces;
using Shield.HardwareCom.Models;
using Shield.Data;

namespace Shield.HardwareCom
{
    public class HardwareComAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Models registration (single interface per model)
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.Name.EndsWith("Model"))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // System internals registration - MicroSoft
            builder.RegisterType<SerialPort>().AsSelf();

            // Factories registration (single interface per factory) both normal and autofac's factories
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Except<CommunicationDeviceFactory>(icdf => icdf.As<ICommunicationDeviceFactory>().SingleInstance())
                   .Where(t => t.Name.EndsWith("Factory"))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));
            
            #region Communication Device Factory
            
            builder.RegisterType<SerialPortAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Serial)
                   .WithParameters(new []{
                                   new ResolvedParameter(
                                       (pi, ctx) => pi.ParameterType == typeof(SerialPort) && pi.Name == "port",
                                       (pi, ctx) => ctx.Resolve<SerialPort>()), 
                                   new ResolvedParameter(
                                       (pi, ctx) => pi.ParameterType == typeof(Func<ICommandModel>) && pi.Name == "commandModelFac",
                                       (pi, ctx) => ctx.Resolve<Func<ICommandModel>>()),
                                   new ResolvedParameter(
                                       (pi, ctx) => pi.ParameterType == typeof(IAppSettings) && pi.Name == "appSettings",
                                       (pi, ctx) => ctx.Resolve<IAppSettings>()),
                                   new ResolvedParameter(
                                       (pi, ctx) => pi.ParameterType == typeof(ICommandTranslator) && pi.Name == "commandTranslator",
                                       (pi, ctx) => ctx.Resolve<ICommandTranslator>())});

            builder.RegisterType<MoqAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Moq)
                   .WithParameter(new ResolvedParameter(
                                 (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "portName",
                                 (pi, ctx) => "1"));
            #endregion

            // Working classes
            builder.RegisterType<CommandTranslator>()
                   .As<ICommandTranslator>()
                   .WithParameters(new[]{
                                   new ResolvedParameter(
                                       (pi, ctx) => pi.ParameterType == typeof(IAppSettings) && pi.Name == "appSettings",
                                       (pi, ctx) => ctx.Resolve<IAppSettings>()),
                                   new ResolvedParameter(
                                       (pi, ctx) => pi.ParameterType == typeof(Func<ICommandModel>) && pi.Name == "commandModelFac",
                                       (pi, ctx) => ctx.Resolve<Func<ICommandModel>>())});

            // tymczasowo do wszystkiego innego
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Except<IMessanger>()
                   .Except<Messanger>()
                   .Except<MoqAdapter>()
                   .Except<SerialPortAdapter>()
                   .Except<CommandTranslator>()
                   .Except<CommunicationDeviceFactory>()
                   .AsImplementedInterfaces()
                   .InstancePerDependency();
            
            base.Load(builder);
        }
    }
}
