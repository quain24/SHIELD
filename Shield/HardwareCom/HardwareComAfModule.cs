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
                   .Except<CommandModelFactory>()
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

            // Command factory

            builder.RegisterType<CommandModelFactory>()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(int) && pi.Name == "idLength",
                           (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().IdSize)
                   })
                   .As<ICommandModelFactory>();

            // Message factory

            builder.RegisterType<MessageFactory>()
                   .WithParameter(
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(int) && pi.Name == "idLength",
                           (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().IdSize)
                   )
                   .As<IMessageFactory>();

            // End of factories registration ========================================================================================================

            // Working classes
            builder.RegisterType<CommandTranslator>()
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

                        var d = c.ResolveNamed<ITimeoutCheck>("completition" + nameof(TimeoutCheck));

                        return incomingDataPreparer;
                    })
                   .As<IIncomingDataPreparer>();

            builder.RegisterType<Messenger>()
                   .As<IMessanger>();

            // MESSAGE PROCESSING: =====================================================================================

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

            builder.RegisterType<TimeoutCheck>()
                   .WithParameter("timeout", 0)
                   .Named<ITimeoutCheck>(nameof(TimeoutCheck));

            builder.RegisterType<TimeoutCheck>()
                   .WithParameter(new ResolvedParameter(
                       (pi, ctx) => pi.ParameterType == typeof(long) && pi.Name == "timeout",
                       (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().CompletitionTimeout))
                   .Named<ITimeoutCheck>("completition" + nameof(TimeoutCheck));

            builder.RegisterType<TimeoutCheck>()
                   .WithParameter(new ResolvedParameter(
                       (pi, ctx) => pi.ParameterType == typeof(long) && pi.Name == "timeout",
                       (pi, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().ConfirmationTimeout))
                   .Named<ITimeoutCheck>("confirmation" + nameof(TimeoutCheck));

            builder.RegisterType<ConfirmationTimeoutChecker>()
                   .As<IConfirmationTimeoutChecker>()
                   .WithParameter(new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(ITimeoutCheck) && pi.Name == "timeoutCheck",
                            (pi, ctx) => ctx.ResolveNamed<ITimeoutCheck>("confirmation" + nameof(TimeoutCheck))));

            #endregion Classes for checking correctness

            // TEST =============

            builder.RegisterType<Inherittest>()
                   .As<IInherittest>()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(ITimeoutCheck) && pi.Name == "completitionCheck",
                            (pi, ctx) => ctx.Resolve<ITimeoutCheck>(new ResolvedParameter(
                                (pii, ctxx) => pii.ParameterType == typeof(long) && pii.Name == "timeout",
                                (pii, ctxx) => ctxx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().CompletitionTimeout))),

                        new ResolvedParameter(
                            (pi, ctx) => pi.ParameterType == typeof(ITimeoutCheck) && pi.Name == "confirmationCheck",
                            (pi, ctx) => ctx.Resolve<ITimeoutCheck>(new ResolvedParameter(
                                (pii, ctxx) => pii.ParameterType == typeof(long) && pii.Name == "timeout",
                                (pii, ctxx) => ctxx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().ConfirmationTimeout)))
                   });

            #region Message object processing

            builder.RegisterType<CommandIngester>()
                   .As<ICommandIngester>()
                   .WithParameter(
                       new ResolvedParameter(
                           (pi, ctx) => pi.ParameterType == typeof(ITimeoutCheck) && pi.Name == "completitionTimeout",
                           (pi, ctx) => ctx.ResolveNamed<ITimeoutCheck>("completition" + nameof(TimeoutCheck)))
                   );

            #endregion Message object processing

            // Additional objects, some may be temporary

            //builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
            //    .Where(t => t.IsInNamespace($@"Shield.HardwareCom.MessageProcessing"))
            //    .AsImplementedInterfaces();

            //base.Load(builder);
        }
    }
}