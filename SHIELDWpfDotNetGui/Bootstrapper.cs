using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using SHIELDGui.ViewModels;

namespace SHIELDGui
{
    public class Bootstrapper : BootstrapperBase
    {
        // konf simple container, continued on bottom 
        private SimpleContainer _container = new SimpleContainer();
        
        // Initialize - requires additional config in app.xaml (resourceDictionary... etc bootstrapper)
        public Bootstrapper()
        {
            Initialize();
        }

        // Start this window after boot
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }


        #region Simple Container configuration 
        protected override void Configure()
        {
            _container.Instance(_container);

            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>();

            GetType().Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType =>
                         _container.RegisterPerRequest(viewModelType, viewModelType.ToString(), viewModelType));
        }
        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        } 
        #endregion
    }
}
