using Autofac;
using Autofac.Core;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.PartValidators;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Shield.Enums.Command;

namespace Shield.WpfGui.AutofacModules
{
    public class MessagingAfModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var MessagingAssembly = Assembly.Load(typeof(Messaging.Commands.Command).Assembly.FullName);

            builder.RegisterType<IDPart>()
                .Named<IPart>("IDPart");

            builder.RegisterType<OnlyAlphanumericAllowedValidator>()
                .WithParameter(new NamedParameter("length", 4))
                .Named<IPartValidator>("AllwaysGoodValidator")
                .SingleInstance();


            builder.RegisterType<PartValidatorBuilder>()
                .As<IPartValidatorBuilder>();

            // TODO every part validator should be registered like that - so it can be used when creating part factories
            builder.Register(c => 
            {
                return c.Resolve<IPartValidatorBuilder>()
                        .MinimumLength(10)
                        .MaximumLength(10)
                        .ForbidChars('*')
                        .ForbidChars('#')
                        .Build();
            })
            .Named<IPartValidator>("DataPartValidator");
                


            // TODO this registration works - factory class should use auto keyed collection of those generic factories.
            builder.RegisterType<PartFactoryAutofacAdapter>()
                .WithParameters(new[]{new ResolvedParameter((pi, ctx) =>pi.Name == "factory", (pi, ctx) => ctx.ResolveNamed<Func<string, IPartValidator, IPart>>("IDPart")),
                               new ResolvedParameter((pi, ctx) =>pi.Name == "validator", (pi, ctx) => ctx.ResolveNamed<IPartValidator>("DataPartValidator")) })
                .Keyed<PartFactoryAutofacAdapter>(PartType.ID);

            builder.RegisterType<CommandFactoryAutoFacAdapter>()
                .WithParameter(new ResolvedParameter((pi, ctx) => pi.Name == "commandFactory",
                                                     (pi, ctx) => ctx.Resolve<Func<IPart, IPart, IPart, IPart, IPart, ICommand>>()))
                .AsSelf();

            builder.RegisterType<PrecisePartFactory>()
                .WithParameter(new ResolvedParameter((pi, ctx) => pi.Name == "factories", (pi, ctx) => ctx.Resolve<IReadOnlyDictionary<PartType, PartFactoryAutofacAdapter>>()))
                .AsSelf();
        }
    }
}