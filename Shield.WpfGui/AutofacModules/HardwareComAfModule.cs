using Autofac;
using Autofac.Core;
using Shield.CommonInterfaces;
using Shield.Enums;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.RawDataProcessing;
using Shield.HardwareCom;
using Shield.HardwareCom.Helpers;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Shield.HardwareCom.Adapters;
using System;
using Shield.HardwareCom.Models;

namespace Shield.WpfGui.AutofacModules
{
    // TODO implement host id in commands - creation, translation etc. Modify id generation
    public class HardwareComAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var hardwareComAssembly = Assembly.Load(typeof(SerialPortAdapter).Assembly.FullName);

            // Models registration (single interface per model)
            builder.RegisterAssemblyTypes(hardwareComAssembly)
                   .Where(t => t.IsInNamespace("Shield.HardwareCom") && t.Name.EndsWith("Model", System.StringComparison.Ordinal))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // Factories registration both normal and autofac's factories
            // More complicated factories (with parameters in constructor) are separated below.
            // ==================================================================================================================================
            builder.RegisterAssemblyTypes(hardwareComAssembly)
                   .Where(t => t.IsInNamespace("Shield.HardwareCom.Factories") && t.Name.EndsWith("Factory", System.StringComparison.Ordinal))
                   .Except<CommunicationDeviceFactory>(icdf => icdf.As<ICommunicationDeviceFactory>().SingleInstance())
                   .Except<NormalTimeoutFactory>()
                   .Except<NullTimeoutFactory>()
                   .Except<TimeoutFactory>()
                   .Except<ConfirmationFactory>(cf => cf.As<IConfirmationFactory>().WithParameter(new ResolvedParameter((pi, _) => pi.Name == "hostId", (_, ctx) => ctx.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>().HostId)))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            #region Communication Device Factory and required devices

            builder.RegisterType<SerialPortAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Serial)
                   .UsingConstructor();

            builder.RegisterType<MoqAdapter>()
                   .Keyed<ICommunicationDevice>(DeviceType.Moq)
                   .WithParameter(new ResolvedParameter(
                                 (pi, _) => pi.ParameterType == typeof(string) && pi.Name == "portName",
                                 (_, __) => "1"));

            #endregion Communication Device Factory and required devices

            #region Timeout factory and required entities

            builder.RegisterType<NormalTimeout>();
            builder.RegisterType<NullTimeout>()
                   .SingleInstance();

            builder.RegisterType<NormalTimeoutFactory>()
                   .Keyed<ITimeoutConcreteFactory>(TimeoutType.NormalTimeout)
                   .SingleInstance();
            builder.RegisterType<NullTimeoutFactory>()
                   .Keyed<ITimeoutConcreteFactory>(TimeoutType.NullTimeout)
                   .SingleInstance();

            builder.RegisterType<TimeoutFactory>()
                   .As<ITimeoutFactory>()
                   .SingleInstance();

            #endregion Timeout factory and required entities

            #region MessengingPipelineFactory factory and required entities

            builder.RegisterType<MessengingPipelineFactory>()
                   .As<IMessengingPipelineFactory>();

            builder.RegisterType<MessagingPipelineContext>()
                   .As<IMessagingPipelineContext>();

            builder.RegisterType<MessagingPipeline>()
                   .As<IMessagingPipeline>();

            #endregion MessengingPipelineFactory factory and required entities

            // End of factories registration ========================================================================================================

            // Working classes
            builder.Register(c =>
                {
                    IApplicationSettingsModel appSet = c.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>();
                    var settings = new CommandTranslatorSettings(appSet.Separator,
                                                                 appSet.Filler,
                                                                 appSet.CommandTypeSize,
                                                                 appSet.IdSize,
                                                                 appSet.DataSize,
                                                                 appSet.HostIdSize,
                                                                 appSet.HostId);

                    return new CommandTranslator(settings, c.Resolve<ICommandModelFactory>());
                })
                .As<ICommandTranslator>();

            builder.Register(c =>
                    {
                        IApplicationSettingsModel appSet = c.Resolve<ISettings>().ForTypeOf<IApplicationSettingsModel>();
                        return new IncomingDataPreparer(appSet.CommandTypeSize,
                                                      appSet.IdSize,
                                                      appSet.HostIdSize,
                                                      appSet.DataSize,
                                                      new Regex($"[{appSet.Separator}][a-zA-Z0-9]{{{appSet.HostIdSize}}}[{appSet.Separator}][0-9]{{{appSet.CommandTypeSize}}}[{appSet.Separator}][a-zA-Z0-9]{{{appSet.IdSize}}}[{appSet.Separator}]"),
                                                      appSet.Separator);
                    })
                   .As<IIncomingDataPreparer>();

            // MESSAGE PROCESSING: ===================================================================================================================

            #region Classes for checking correctness

            builder.RegisterType<TypeDetectorAnalyzer>()
                   .As<IMessageAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerType.TypeDetector);

            builder.RegisterType<PatternAnalyzer>()
                   .As<IMessageAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerType.Pattern);

            builder.RegisterType<DecodingAnalyzer>()
                   .As<IMessageAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerType.Decoding);

            builder.RegisterType<IncomingMessageProcessor>()
                   .As<IIncomingMessageProcessor>();

            builder.RegisterType<Completeness>()
                   .As<ICompleteness>();

            builder.RegisterType<CommandIngester>()
                   .As<ICommandIngester>();

            #endregion Classes for checking correctness

            #region Message object processing

            builder.RegisterType<CompletitionTimeoutChecker>()
                   .As<ICompletitionTimeoutChecker>();

            builder.RegisterType<ConfirmationTimeoutChecker>()
                   .As<IConfirmationTimeoutChecker>();

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