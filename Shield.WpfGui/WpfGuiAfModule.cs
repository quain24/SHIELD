using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;

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
                .AsSelf();

            builder.RegisterAssemblyTypes(Assembly.Load($"{nameof(Shield)}.{nameof(WpfGui)}"))
                .Where(t => t.IsInNamespace("Shield.WpfGui.Models") && t.Name.EndsWith("Model"))
                .AsSelf();
                
            base.Load(builder);
        }
    }
}
