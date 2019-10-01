using Autofac;
using System.Linq;
using System.Reflection;

namespace Shield.ConsoleUI
{
    public class ConsoleUIAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.Load($"{nameof(Shield)}.{nameof(ConsoleUI)}"))
                   .Except<Application>(app => app.As<IApplication>())
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            base.Load(builder);
        }
    }
}