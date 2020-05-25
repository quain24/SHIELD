using Autofac;
using Shield.Commands.Parts;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;
using System;
using System.Collections.Generic;
using System.Linq;
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

            builder.RegisterType<AllAlphanumericAllowedValidator>()
                .WithParameter(new NamedParameter("length", 4))
                .Named<IPartValidator>("AllwaysGoodValidator")
                .SingleInstance();

            builder.Register(c =>
            {
                var dep1 = c.Resolve<DependencyDictionary<string, Func<string, IPartValidator, IPart>>>();
                var dep2 = c.Resolve<DependencyDictionary<string, Func<string, IPartValidator>>>();

                var t = dep1["IDPart"].Invoke("test", dep2["AllwaysGoodValidator"].Invoke(""));

                return t;
            })
            .As<IPart>();

            IDictionary<PartType, Tuple<IPartValidator, Func<string, IPartValidator, IPart>>> _combinedPartValidatorPairs = null;

            // TODO what to push as a parameter?
        }
    }
}
