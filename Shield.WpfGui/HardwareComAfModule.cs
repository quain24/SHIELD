using Autofac;
using System.Reflection;

namespace Shield.HardwareCom
{
    public class HardwareComAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.Load($"{nameof(Shield)}.{nameof(HardwareCom)}"))
                //.Except<SerialPortAdapterFactory>(cpf => cpf.As<SerialPortAdapterFactory>().SingleInstance())
                .AsImplementedInterfaces()
                .InstancePerDependency();
        }
    }
}