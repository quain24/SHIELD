using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using System.Reflection;

namespace TemporaryConsoleUI
{
    public class TemporaryConsoleUIAutoFacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(TemporaryConsoleUI)))
                   .Except<Application>(app => app.As<IApplication>())             // Zamiast osobnego builder.register
                   .Where(t => t.Name.Contains("COM") || t.Name.Contains("Com"))   // Konieczne by urzyć pózniej As z parametrami zamiast AsImplementedInterfaces
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));
            //.AsImplementedInterfaces()
            //.InstancePerDependency();

            base.Load(builder);
        }
    }
}
