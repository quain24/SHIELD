using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using System.Reflection;

namespace CommunicationManager
{
    public class CommunicationManagerAutoFacModule : Autofac.Module
    { 
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(CommunicationManager)))
                   //.Where(t => t.Namespace.Contains("Model"))
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            base.Load(builder);
        }
    }
}
