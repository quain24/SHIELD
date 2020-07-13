using Autofac;
using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shield.WpfGui.AutofacModules
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

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.IsInNamespace("Shield.WpfGui.ViewModels") && t.Name.EndsWith("ViewModel"))
                .AsSelf();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.IsInNamespace("Shield.WpfGui.Models") && t.Name.EndsWith("Model"))
                .AsSelf();

            // Replacement for AutoFac builtIn IIndex for better separation
            builder.RegisterGeneric(typeof(DependencyDictionary<,>))
                   .As(typeof(IReadOnlyDictionary<,>));

            base.Load(builder);
        }
    }
}