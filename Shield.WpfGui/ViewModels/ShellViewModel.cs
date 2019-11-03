using Caliburn.Micro;
using Shield.HardwareCom.Adapters;
using System;
using System.Diagnostics;
using Shield;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.Data;
using Shield.Helpers;
using Shield.Extensions;
using Shield.CommonInterfaces;
using Shield.HardwareCom.Factories;
using Shield.Data.Models;
using Autofac.Features.Indexed;

namespace Shield.WpfGui.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        Messenger _messenger;
        ComCommander _comCommander;
        CommunicationDeviceFactory _comDevFac;
        AppSettings _setMan;

        public ShellViewModel()
        {
            _setMan = new AppSettings(new AppSettingsModel());
            _setMan.LoadFromFile();
                        
            _comDevFac = new CommunicationDeviceFactory(devfac, _setMan);

            _messenger = new Messenger()

        }

        
    }
}