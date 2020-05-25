using Autofac;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Shield.WpfGui.AutofacModules
{
    public class ShieldAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Models registration (single interface per model)
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.Name.EndsWith("Model") && t.Namespace == "Shield")
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            // Factories registration (single interface per factory) both normal and autofac's factories
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.Name.EndsWith("Factory") && t.Namespace == "Shield")
                   .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));

            

            // tymczasowo do wszystkiego innego
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(Shield)))
                   .Where(t => t.Namespace == "Shield")
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            base.Load(builder);
        }
    }
}