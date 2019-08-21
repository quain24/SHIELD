using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using System.Reflection;
using CommunicationManager.Models;

namespace CommunicationManager
{
    public class CommunicationManagerAutoFacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.Load(nameof(CommunicationManager)))
                   .Except<CommandModel>(comm => comm.As<ICommandModel>()
                        // Testowe nadanie domyślnych parametrów dla konctruktora klasy CommandModel
                        .WithParameters(new[]
                        {
                            new ResolvedParameter(
                                (pi, ctx) => pi.ParameterType == typeof(CommandModel.Type) && pi.Name == "commandType",
                                (pi, ctx) => 1),
                            new ResolvedParameter(
                                (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "command",
                                (pi, ctx) => "Test string injected into command parameter")
                        }))
                   //.Where(t => t.Namespace.Contains("Model"))
                   .AsImplementedInterfaces()
                   .InstancePerDependency();

            builder.RegisterType<System.IO.Ports.SerialPort>().AsSelf().InstancePerDependency();

            base.Load(builder);
        }
    }
}
