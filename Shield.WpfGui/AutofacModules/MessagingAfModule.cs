using Autofac;
using Autofac.Core;
using Shield.Commands.Parts;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using static Shield.Command;

namespace Shield.WpfGui.AutofacModules
{
    public class MessagingAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var MessagingAssembly = Assembly.Load(typeof(Messaging.Commands.Command).Assembly.FullName);

            builder.RegisterType<IDPart>()
                .Named<IPart>("IDPart");
            builder.RegisterType<TypePart>()
                .Named<IPart>("TypePart");

            builder.RegisterType<AllAlphanumericAllowedValidator>()
                .WithParameter(new NamedParameter("length", 4))
                .Named<IPartValidator>("AllwaysGoodValidator")
                .SingleInstance();

            // TODO this registration works - factory class should use auto keyed collection of those generic factories.
            builder.RegisterType<PartFactoryAutofacAdapter>()
                .WithParameters(new []{new ResolvedParameter((pi, ctx) =>pi.Name == "factory", (pi, ctx) => ctx.ResolveNamed<Func<string, IPartValidator, IPart>>("IDPart")),
                               new ResolvedParameter((pi, ctx) =>pi.Name == "validator", (pi, ctx) => ctx.ResolveNamed<IPartValidator>("AllwaysGoodValidator")) })
                .Keyed<PartFactoryAutofacAdapter>(PartType.ID);

            builder.RegisterType<PartFactoryAutofacAdapter>()
                .WithParameters(new[]{new ResolvedParameter((pi, ctx) =>pi.Name == "factory", (pi, ctx) => ctx.ResolveNamed<Func<string, IPartValidator, IPart>>("TypePart")),
                               new ResolvedParameter((pi, ctx) =>pi.Name == "validator", (pi, ctx) => ctx.ResolveNamed<IPartValidator>("AllwaysGoodValidator")) })
                .Keyed<PartFactoryAutofacAdapter>(PartType.Type);

            builder.RegisterType<PartFactory>()
                .WithParameter(new ResolvedParameter((pi, ctx) => pi.Name == "factories", (pi, ctx) => ctx.Resolve<IReadOnlyDictionary<PartType, PartFactoryAutofacAdapter>>()))
                .AsSelf();

        }
    }
}
