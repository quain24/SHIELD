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

            builder.RegisterModule<ConsoleUIAfModule>();
            //builder.RegisterModule<Shield. HardwareComAfModule>();
            builder.RegisterModule<DataAfModule>();
            builder.RegisterModule<ShieldAfModule>();

            return builder.Build();
        }
    }
}