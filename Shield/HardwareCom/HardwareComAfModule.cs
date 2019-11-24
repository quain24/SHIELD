using Autofac;
using Autofac.Core;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Adapters;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom
{
    public class HardwareComAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Models registration (single interface per model)
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.IsInNamespace("Shield.HardwareCom") && t.Name.EndsWith("Model"))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // Factories registration (single interface per factory) both normal and autofac's factories
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.IsInNamespace("Shield.HardwareCom") && t.Name.EndsWith("Factory"))
                   .Except<CommunicationDeviceFactory>(icdf => icdf.As<ICommunicationDeviceFactory>().SingleInstance())
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            #region Communication Device Factory

            builder.RegisterType<SerialPortAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Serial)
                   .UsingConstructor();  // use parameterless constructor when using setup in factory

            // can be used when instead of additional call for setup method when creating this device in communicationDeviceFactory
            //.WithParameter(new ResolvedParameter(
            //              (pi, ctx) => pi.ParameterType == typeof(ISerialPortSettingsModel) && pi.Name == "settings",
            //              (pi, ctx) => ctx.Resolve<IAppSettings>().GetSettingsFor<ISerialPortSettingsModel>()));

            builder.RegisterType<MoqAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Moq)
                   .WithParameter(new ResolvedParameter(
                                 (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "portName",
                                 (pi, ctx) => "1"));

            #endregion Communication Device Factory

            // Working classes
            builder.RegisterType<CommandTranslator>()
                   .As<ICommandTranslator>()
                   .WithParameters(new[]
                   {
                        new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(ISettings) && pi.Name == "appSettings",
                            (pi, ctx) => ctx.Resolve<ISettings>()),
                        new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(Func<ICommandModel>) && pi.Name == "commandModelFac",
                            (pi, ctx) => ctx.Resolve<Func<ICommandModel>>()),
                        new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(ICommandTranslator) && pi.Name == "commandTranslator",
                            (pi, ctx) => ctx.Resolve<ICommandTranslator>())
                   });

            builder.Register(c =>
                    {
                        IApplicationSettingsModel appSet = c.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>();
                        IIncomingDataPreparer incomingDataPreparer =
                            new IncomingDataPreparer(appSet.CommandTypeSize,
                                                     appSet.IdSize,
                                                     appSet.DataSize,
                                                     new Regex($@"[{appSet.Separator}][0-9]{{{appSet.CommandTypeSize}}}[{appSet.Separator}][a-zA-Z0-9]{{{appSet.IdSize}}}[{appSet.Separator}]"),
                                                     appSet.Separator);
                        return incomingDataPreparer;
                    })
                   .As<IIncomingDataPreparer>();

            builder.RegisterType<Messenger>()
                   .As<IMessanger>()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(ICommunicationDeviceFactory) && pi.Name == "communicationDeviceFactory",
                           (pi, ctx) => ctx.Resolve<ICommunicationDeviceFactory>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(ICommandTranslator) && pi.Name == "commandTranslator",
                           (pi, ctx) => ctx.Resolve<ICommandTranslator>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(IIncomingDataPreparer) && pi.Name == "incomingDataPreparer",
                           (pi, ctx) => ctx.Resolve<IIncomingDataPreparer>())
                   });


            // Additional objects, some may be temporary

            builder.RegisterType<MessageInfoAndErrorChecks>()
                .As<IMessageInfoAndErrorChecks>();


            // tymczasowo do wszystkiego innego
            //builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
            //       .Where(t => t.IsInNamespace("HardwareCom"))
            //       .Except<Messenger>()
            //       .Except<MoqAdapter>()
            //       .Except<SerialPortAdapter>()
            //       .Except<CommandTranslator>()
            //       .Except<CommunicationDeviceFactory>()
            //       .Except<IncomingDataPreparer>()
            //       .AsImplementedInterfaces()
            //       .InstancePerDependency();

            base.Load(builder);
        }
    }
}