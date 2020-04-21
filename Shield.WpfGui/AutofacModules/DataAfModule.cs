using Autofac;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using System.Linq;
using System.Reflection;

namespace Shield.WpfGui.AutofacModules
{
    public class DataAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Assembly current = Assembly.Load(nameof(Shield));

            // Models registration (general)
            builder.RegisterAssemblyTypes(current)
                   .Where(t => t.IsInNamespace("Shield.Data") && t.Name.EndsWith("Model", System.StringComparison.Ordinal))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // Factories registration (single interface per factory) both normal and autofac's factories
            builder.RegisterAssemblyTypes(current)
                   .Where(t => t.IsInNamespace("Shield.Data") && t.Name.EndsWith("Factory", System.StringComparison.Ordinal))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            builder.RegisterType<Settings>().As<ISettings>().SingleInstance();

            builder.RegisterType<CommunicationDeviceSettingsContainer>()
                   .AsSelf()
                   .As<ICommunicationDeviceSettingsContainer>()
                   .Named<ISetting>("SerialPortSettingsContainer");

            base.Load(builder);
        }
    }
}