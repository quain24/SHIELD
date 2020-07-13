using Autofac;
using Shield.CommonInterfaces;
using Shield.Persistence;
using Shield.Persistence.Models;
using System.Linq;
using System.Reflection;

namespace Shield.WpfGui.AutofacModules
{
    public class DataAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var dataAssembly = Assembly.Load(typeof(ApplicationSettingsModel).Assembly.FullName);

            // Models registration (general)
            builder.RegisterAssemblyTypes(dataAssembly)
                   .Where(t => t.IsInNamespace("Shield.Persistance") && t.Name.EndsWith("Model", System.StringComparison.Ordinal))
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // Factories registration (single interface per factory) both normal and autofac's factories
            builder.RegisterAssemblyTypes(dataAssembly)
                   .Where(t => t.IsInNamespace("Shield.Persistance") && t.Name.EndsWith("Factory", System.StringComparison.Ordinal))
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