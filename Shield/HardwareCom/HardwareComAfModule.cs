using Autofac;
using Autofac.Core;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.HardwareCom.Adapters;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;
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
                   .Except<TimeoutCheckFactory>()
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
                   .WithParameter(
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(IIdGenerator) && pi.Name == "idGenerator",
                           (pi, ctx) => ctx.Resolve<IIdGenerator>())
                   )
                   .As<IMessageFactory>();

            // TimeOutCheck factory

            builder.RegisterType<NullTimeoutCheck>()
                   .Named<ITimeoutCheck>("Null" + nameof(TimeoutCheck))
                   .SingleInstance();

            builder.RegisterInstance<Func<int, ITimeoutCheck>>(timeout => new TimeoutCheck(timeout));

            builder.RegisterType<TimeoutCheckFactory>()
                   .AsSelf()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "timeoutAutoFactory",
                           (pi, ctx) => ctx.Resolve<Func<int, ITimeoutCheck>>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "nullTimeoutCheck",
                           (pi, ctx) => ctx.ResolveNamed<ITimeoutCheck>("Null" + nameof(TimeoutCheck)))
                   });

            builder.Register(c => new TimeoutCheck(c.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().CompletitionTimeout))
                   .Named<ITimeoutCheck>("completition" + nameof(TimeoutCheck));

            builder.Register(c => new TimeoutCheck(c.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().ConfirmationTimeout))
                   .Named<ITimeoutCheck>("confirmation" + nameof(TimeoutCheck));

            // End of factories registration ========================================================================================================

            // Working classes
            builder.RegisterType<CommandTranslator>()
                   .WithParameter(new ResolvedParameter(
                       (pi, ctx) => pi.Name == "applicationSettings",
                       (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>()))
                   .As<ICommandTranslator>();

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
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.Name == "device",
                    (pi, ctx) => ctx.Resolve<ICommunicationDeviceFactory>().CreateDevice(ctx.Resolve<ISettings>().ForTypeOf<ISerialPortSettingsContainer>().GetSettingsByPortNumber(4))))
                   .As<IMessenger>();

            builder.RegisterType<MessengingPipeline>()
                   .WithParameter(new ResolvedParameter(
                       (pi, ctx) => pi.Name == "device",
                       (pi, ctx) => ctx.Resolve<ICommunicationDeviceFactory>().CreateDevice(
                           ctx.Resolve<ISettings>().ForTypeOf<ISerialPortSettingsContainer>().GetSettingsByPortNumber(4))))
                   .AsSelf();

            // MESSAGE PROCESSING: ===================================================================================================================

            #region Classes for checking correctness

            builder.RegisterType<TypeDetectorAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerTypes.TypeDetector);

            builder.RegisterType<PatternAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerTypes.Pattern);

            builder.RegisterType<DecodingAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerTypes.Decoding);

            builder.RegisterType<IncomingMessageProcessor>()
                   .As<IIncomingMessageProcessor>()
                   .WithParameter(
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "analyzers",
                           (pi, ctx) => new[]
                           {
                               ctx.ResolveKeyed<IMessageAnalyzer>(MessageAnalyzerTypes.Decoding),
                               ctx.ResolveKeyed<IMessageAnalyzer>(MessageAnalyzerTypes.Pattern),
                               ctx.ResolveKeyed<IMessageAnalyzer>(MessageAnalyzerTypes.TypeDetector)
                           }
                       )
                   );

            builder.RegisterType<Completeness>()
                   .As<ICompleteness>();

            builder.RegisterType<ConfirmationTimeoutChecker>()
                   .WithParameter(new ResolvedParameter(
                            (pi, ctx) => pi.Name == "timeoutCheck",
                            (pi, ctx) => ctx.Resolve<TimeoutCheckFactory>().GetTimeoutCheckWithTimeoutSetTo(ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().ConfirmationTimeout)))
                   .As<IConfirmationTimeoutChecker>();

            #endregion Classes for checking correctness
            
            #region Message object processing

            builder.RegisterType<CommandIngester>()
                   .As<ICommandIngester>()
                   .WithParameter(
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(ITimeoutCheck) && pi.Name == "completitionTimeout",
                           (pi, ctx) => ctx.Resolve<TimeoutCheckFactory>().GetTimeoutCheckWithTimeoutSetTo(
                                            ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().ConfirmationTimeout))
                   );

            #endregion Message object processing

            // HELPERS ===============================================================================================================================

            builder.Register(c =>
                {
                    var idLenght = c.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().IdSize;
                    return new IdGenerator(idLenght);
                })
                .As<IIdGenerator>();
        }
    }
}