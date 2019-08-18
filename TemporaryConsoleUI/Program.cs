using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemporaryConsoleUI
{
    public class Program
    {
        static void Main(string[] args)
        {
            var container = ContainerConfig.Configure();

            // App is starting, giving controll to Run method from now on.
            using(var scope = container.BeginLifetimeScope())
            {
                var app = scope.Resolve<IApplication>();
                app.Run();
            }

            Console.ReadLine();
        }
    }
}
