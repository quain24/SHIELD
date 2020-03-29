using Autofac;
using Caliburn.Micro;
using Shield.Data;
using Shield.HardwareCom;
using Shield.WpfGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Shield.WpfGui
{
    public class Bootstrapper : BootstrapperBase
    {
        // konf simple container, continued on bottom
        private static IContainer _container;

        // Initialize - requires additional config in app.xaml (resourceDictionary... etc bootstrapper)
        public Bootstrapper()
        {
            Initialize();
        }

        // Start this window after boot
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();

            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
        }

        public class DebugTraceListener : TraceListener
        {
            public override void Write(string message)
            {
            }

            public override void WriteLine(string message)
            {
                Debugger.Break();
            }
        }

        #region AutoFac Configuration

        protected override void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<HardwareComAfModule>();
            builder.RegisterModule<DataAfModule>();
            builder.RegisterModule<ShieldAfModule>();
            builder.RegisterModule<WpfGuiAfModule>();

            _container = builder.Build();
        }

        protected override object GetInstance(Type service, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                if (_container.IsRegistered(service))
                    return _container.Resolve(service);
            }
            else
            {
                if (_container.IsRegisteredWithKey(key, service))
                    return _container.ResolveKeyed(key, service);
            }

            var msgFormat = "Could not locate any instances of contract {0}.";
            var msg = string.Format(msgFormat, key ?? service.Name);
            throw new Exception(msg);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(service);
            return _container.Resolve(type) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }

        #endregion AutoFac Configuration
    }
}