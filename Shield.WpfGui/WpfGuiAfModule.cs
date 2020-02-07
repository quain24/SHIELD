using Autofac;
using Caliburn.Micro;
using System.Linq;
using System.Reflection;
using Shield.WpfGui.ViewModels;
using Autofac.Core;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using System;
using Shield.HardwareCom.Models;

namespace Shield.WpfGui
{
    public class WpfGuiAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>()
                .As<IWindowManager>()
                .SingleInstance();

            builder.RegisterType<EventAggregator>()
                .As<IEventAggregator>()
                .SingleInstance();

            builder.RegisterAssemblyTypes(Assembly.Load($"{nameof(Shield)}.{nameof(WpfGui)}"))
                .Where(t => t.IsInNamespace("Shield.WpfGui.ViewModels") && t.Name.EndsWith("ViewModel"))
                .Except<ShellViewModel>()
                .AsSelf();

            builder.RegisterAssemblyTypes(Assembly.Load($"{nameof(Shield)}.{nameof(WpfGui)}"))
                .Where(t => t.IsInNamespace("Shield.WpfGui.Models") && t.Name.EndsWith("Model"))
                .AsSelf();

            builder.RegisterType<ShellViewModel>()
                   .WithParameters(new[]
                   {
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "messanger",
                           (pi, ctx) => ctx.Resolve<HardwareCom.IMessanger>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "settings",
                           (pi, ctx) => ctx.Resolve<Data.ISettings>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "messageFactory",
                           (pi, ctx) => ctx.Resolve<Func<IMessageModel>>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "commandFactory",
                           (pi, ctx) => ctx.Resolve<HardwareCom.Factories.ICommandModelFactory>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "commandIngester",
                           (pi, ctx) => ctx.Resolve<HardwareCom.ICommandIngester>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "incomingMessageProcessor",
                           (pi, ctx) => ctx.ResolveKeyed<HardwareCom.IMessageProcessor>(nameof(HardwareCom.IncomingMessageProcessor))),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "confirmationFactory",
                           (pi, ctx) => ctx.Resolve<IConfirmationFactory>()),
                       new ResolvedParameter(
                           (pi, ctx) => pi.Name == "confirmationTimeoutChecker",
                           (pi, ctx) => ctx.Resolve<IConfirmationTimeoutChecker>())
                   })
                   .AsSelf();

            base.Load(builder);
        }
    }
}