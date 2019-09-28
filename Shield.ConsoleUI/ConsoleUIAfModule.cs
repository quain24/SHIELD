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
                .Except<Application>(app => app.As<IApplication>())             // Zamiast osobnego builder.register
                                                                                //.Where(t => t.Name.Contains("COM") || t.Name.Contains("Com")) // Konieczne by urzyć pózniej As z parametrami zamiast AsImplementedInterfaces
                                                                                //.Where(t => t.Name.Contains("a"))
                                                                                //.As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));
            .AsImplementedInterfaces()
            .InstancePerDependency();

            base.Load(builder);
        }
    }
}