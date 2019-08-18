using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TemporaryConsoleUI
{
    public static class ContainerConfig
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Application>().As<IApplication>();

            builder.RegisterAssemblyTypes(Assembly.Load(nameof(CommunicationManager)))
                //.Where(t => t.Namespace.Contains("Manager"))
                .As(t => t.GetInterfaces().FirstOrDefault(i => i.Name == "I" + t.Name));                

            return builder.Build();
        }
    }
}
