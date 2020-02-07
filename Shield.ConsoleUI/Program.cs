using Autofac;

namespace Shield.ConsoleUI
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var container = ContainerConfig.Configure();

            // App is starting, giving control to Run method from now on.
            using (var scope = container.BeginLifetimeScope())
            {
                var app = scope.Resolve<IApplication>();
                app.Run();
            }
        }
    }
}