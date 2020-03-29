using Autofac;
using Caliburn.Micro;
using Shield.WpfGui.ViewModels;
using System.Linq;
using System.Reflection;

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


            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.IsInNamespace($@"Shield.WpfGui.ViewModels") && t.Name.EndsWith("ViewModel"))
                .AsSelf();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.IsInNamespace($@"Shield.WpfGui.Models") && t.Name.EndsWith("Model"))
                .AsSelf();

            base.Load(builder);
        }
    }
}