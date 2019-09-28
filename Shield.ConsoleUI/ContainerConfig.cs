using Autofac;
using Shield.Data;
using Shield.HardwareCom;

namespace Shield.ConsoleUI
{
    public static class ContainerConfig
    {
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            //builder.RegisterAssemblyTypes(Assembly.Load(nameof(TemporaryConsoleUI)))
            //    .Except<Application>()
            //    .Where(t => t.Name.Contains("COM") || t.Name.Contains("Com"))   // Konieczne by urzyć pózniej As z parametrami zamiast AsImplementedInterfaces
            //    .As(t => t.GetInterfaces().SingleOrDefault(i => i.Name == "I" + t.Name));
            //    //.AsImplementedInterfaces()
            //    //.InstancePerDependency();

            //builder.RegisterType<Application>().As<IApplication>();

            //builder.RegisterAssemblyTypes(Assembly.Load(nameof(CommunicationManager)))
            //    .Where(t => t.Namespace.Contains("Model"))
            //    .As(t => t.GetInterfaces().Single(i => i.Name == "I" + t.Name));

            //builder.RegisterAssemblyTypes(Assembly.Load(nameof(CommunicationManager)))
            ////.Where(t => t.Namespace.Contains("Model"))
            //.AsImplementedInterfaces()
            //.InstancePerDependency();

            builder.RegisterModule<ConsoleUIAfModule>();
            builder.RegisterModule<HardwareComAfModule>();
            builder.RegisterModule<DataAfModule>();
            builder.RegisterModule<ShieldAfModule>();

            return builder.Build();
        }
    }
}