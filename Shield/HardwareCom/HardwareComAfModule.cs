using Autofac;
using Autofac.Core;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Adapters;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
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
            // More complicated factories (with params in constructor) are separated below.
            // ==================================================================================================================================
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.IsInNamespace("Shield.HardwareCom") && t.Name.EndsWith("Factory"))
                   .Except<CommunicationDeviceFactory>(icdf => icdf.As<ICommunicationDeviceFactory>().SingleInstance())
                   .Except<MessageFactory>()
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            #region Communication Device Factory and required devices

            builder.RegisterType<SerialPortAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Serial)
                   .UsingConstructor();

            builder.RegisterType<MoqAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Moq)
                   .WithParameter(new ResolvedParameter(
                                 (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "portName",
                                 (pi, ctx) => "1"));

            #endregion Communication Device Factory and required devices

            // Message factory

            builder.RegisterType<MessageFactory>()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(Func<IMessageHWComModel>) && pi.Name == "messageFactory",
                           (pi, ctx) => ctx.Resolve<Func<IMessageHWComModel>>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(int) && pi.Name == "idLength",
                           (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().IdSize)
                   })
                   .As<IMessageFactory>();

            // End of factories registration ========================================================================================================

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

            // MESSAGE PROCESSING: =====================================================================================

            #region Classes for checking correctness

            builder.RegisterType<Completeness>()
                   .As<ICompleteness>();

            builder.RegisterType<CompletitionTimeout>()
                   .As<ICompletitionTimeout>()
                   .WithParameter(new ResolvedParameter(
                       (pi, ctx) => pi.ParameterType == typeof(long) && pi.Name == "timeout",
                       (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().CompletitionTimeout));

            builder.RegisterType<ConfirmationTimeout>()
                   .As<IConfirmationTimeout>()
                   .WithParameter(new ResolvedParameter(
                       (pi, ctx) => pi.ParameterType == typeof(long) && pi.Name == "timeout",
                       (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().ConfirmationTimeout));

            builder.RegisterType<Decoding>()
                   .As<IDecoding>();

            builder.RegisterType<Pattern>()
                   .As<IPattern>();

            builder.RegisterType<TypeDetector>()
                   .As<ITypeDetector>();

            #endregion Classes for checking correctness

            #region Message object processing

            builder.RegisterType<CommandIngester>()
                   .As<ICommandIngester>()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(IMessageFactory) && pi.Name == "messageFactory",
                            (pi, ctx) => ctx.Resolve<IMessageFactory>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(ICompleteness) && pi.Name == "completeness",
                           (pi, ctx) => ctx.Resolve<ICompleteness>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(ICompletitionTimeout) && pi.Name == "completitionTimeout",
                           (pi, ctx) => ctx.Resolve<ICompletitionTimeout>())
                   });

            builder.RegisterType<MessageProcessor>()
                   .As<IMessageProcessor>()
                   .AsSelf()
                   .Keyed<IMessageProcessor>(nameof(MessageProcessor));

            builder.RegisterType<IncomingMessageProcessor>()
                   .As<IMessageProcessor>()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(IDecoding) && pi.Name == "decoding",
                           (pi, ctx) => ctx.Resolve<IDecoding>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(IPattern) && pi.Name == "pattern",
                           (pi, ctx) => ctx.Resolve<IPattern>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(ITypeDetector) && pi.Name == "typeDetector",
                           (pi, ctx) => ctx.Resolve<ITypeDetector>())
                   })
                   .Keyed<IMessageProcessor>(nameof(IncomingMessageProcessor));

            #endregion Message object processing

            // Additional objects, some may be temporary

            builder.RegisterType<MessageInfoAndErrorChecks>()
                .As<IMessageInfoAndErrorChecks>();

            //builder.RegisterType<CommandIngester>()
            //    .As<ICommandIngester>()
            //    .WithParameter(new ResolvedParameter(
            //        (pi, ctx) => pi.ParameterType == typeof(Func<IMessageHWComModel>) && pi.Name == "messageFactory",
            //        (pi, ctx) => ctx.Resolve<Func<IMessageHWComModel>>()));

            // tymczasowo do wszystkiego innego
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.IsInNamespace("Shield.HardwareCom"))
                   .Except<Messenger>()
                   .Except<MoqAdapter>()
                   .Except<SerialPortAdapter>()
                   .Except<CommandTranslator>()
                   .Except<CommunicationDeviceFactory>()
                   .Except<IncomingDataPreparer>()
                   .Except<MessageFactory>()
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            //builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
            //    .Where(t => t.IsInNamespace($@"Shield.HardwareCom.MessageProcessing"))
            //    .AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}