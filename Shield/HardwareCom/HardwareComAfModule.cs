using Autofac;
using Autofac.Core;
using Autofac.Features.Indexed;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Adapters;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.RawDataProcessing;
using Shield.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom
{
    public class HardwareComAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var shieldAssembly = Assembly.Load(nameof(Shield));


            // Models registration (single interface per model)
            builder.RegisterAssemblyTypes(shieldAssembly)
                   .Where(t => t.IsInNamespace("Shield.HardwareCom") && t.Name.EndsWith("Model"))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // Factories registration both normal and autofac's factories
            // More complicated factories (with parameters in constructor) are separated below.
            // ==================================================================================================================================
            builder.RegisterAssemblyTypes(shieldAssembly)
                   .Where(t => t.IsInNamespace("Shield.HardwareCom.Factories") && t.Name.EndsWith("Factory"))
                   .Except<CommunicationDeviceFactory>(icdf => icdf.As<ICommunicationDeviceFactory>().SingleInstance())
                   .Except<NormalTimeoutFactory>()
                   .Except<NullTimeoutFactory>()
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

            // Timeout factory

            // new timeout mechanism test

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

            builder.RegisterType<CompletitionTimeoutChecker>()
                   .As<ICompletitionTimeoutChecker>();

            builder.RegisterType<ConfirmationTimeoutChecker>()
                   .As<IConfirmationTimeoutChecker>();

            
            // MessagePipeline Factory
            // TODO - clean those dependencies in and out of MessengingPipeline

            builder.RegisterType<MessengingPipelineContext>()
                   .As<IMessengingPipelineContext>();            

            builder.Register(c =>
                        new MessengingPipelineFactory(
                            c.Resolve<IMessengingPipelineContextFactory>()))
                    .AsImplementedInterfaces();


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

            // MESSAGE PROCESSING: ===================================================================================================================

            #region Classes for checking correctness
                       

            builder.RegisterType<TypeDetectorAnalyzer>()
                   .As<IMessageAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerTypes.TypeDetector);

            builder.RegisterType<PatternAnalyzer>()
                   .As<IMessageAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerTypes.Pattern);

            builder.RegisterType<DecodingAnalyzer>()
                   .As<IMessageAnalyzer>()
                   .Keyed<IMessageAnalyzer>(MessageAnalyzerTypes.Decoding);

            builder.RegisterType<IncomingMessageProcessor>()
                   .As<IIncomingMessageProcessor>();

            builder.RegisterType<Completeness>()
                   .As<ICompleteness>();

            builder.RegisterType<CommandIngester>()
                   .As<ICommandIngester>();

            #endregion Classes for checking correctness

            #region Message object processing

            //builder.RegisterType<CommandIngester>()
            //       .As<ICommandIngester>()
            //       .WithParameter(
            //           new ResolvedParameter(
            //               (pi, ctx) => pi.ParameterType == typeof(ITimeoutCheck) && pi.Name == "completitionTimeout",
            //               (pi, ctx) => ctx.Resolve<ITimeoutFactory>().CreateTimeoutWith(
            //                                ctx.Resolve<ISettings>().ForTypeOf<ICommunicationDeviceSettingsContainer>().GetSettingsByDeviceName("COM4").CompletitionTimeout))
            //       );

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