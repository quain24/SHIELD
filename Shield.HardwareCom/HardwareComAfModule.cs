using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Shield.HardwareCom.Factories;

namespace Shield.HardwareCom
{
    public class HardwareComAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.Load($"{nameof(Shield)}.{nameof(HardwareCom)}"))
                .Except<ComPortFactory>(cpf => cpf.As<IComPortFactory>().SingleInstance())      
                .AsImplementedInterfaces()
                .InstancePerDependency();
        }
    }
}
