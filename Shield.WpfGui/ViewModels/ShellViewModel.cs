using Autofac;
using Caliburn.Micro;
using Shield.CommonInterfaces;
using Shield.WpfGui.Validators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Shield.WpfGui.ViewModels
{
    public class ShellViewModel : Conductor<object>, INotifyDataErrorInfo
    {
        private readonly ISettings _settings;
        private readonly string _hostId;
        private string _selectedCommand;
        private string _dataInput;

        private bool _receivingButtonActivated = false;
        private bool _sending = false;
        private bool _openingError = false;

        private CommandDataPackValidation _dataPackValidation;

        private readonly Dictionary<string, ICollection<string>>
        _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ShellViewModel(ISettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _hostId = _settings.ForTypeOf<IApplicationSettingsModel>().HostId;
            _settings.LoadFromFile();
            _settings.SaveToFile();


            _dataPackValidation = new CommandDataPackValidation(_settings.ForTypeOf<IApplicationSettingsModel>().Separator, DataPackFiller());
        }

        public int DataPackLength()
        {
            return _settings.ForTypeOf<IApplicationSettingsModel>().DataSize;
        }

        public char DataPackFiller()
        {
            return _settings.ForTypeOf<IApplicationSettingsModel>().Filler;
        }

        public List<string> DataPackGenerator(string data)
        {
            int dataPackLength = DataPackLength();
            char filler = DataPackFiller();

            List<string> packs = Enumerable.Range(0, data.Length / DataPackLength())
                .Select(i => data.Substring(i * DataPackLength(), DataPackLength()))
                .ToList();

            int packsCumulativeLength = packs.Aggregate(0, (count, val) => count + val.Length);
            if (packsCumulativeLength < data.Length)
            {
                string lastOne = data.Substring(packsCumulativeLength);
                packs.Add(lastOne);
            }

            if (packs.Last().Length < dataPackLength)
                packs[packs.Count - 1] = packs.Last().PadRight(dataPackLength, filler);
            return packs;
        }

        

        public bool CanOpenDevice
        {
            get
            {
                /*if (_pipeline.IsOpen) return false*/;
                return true;
            }
        }

        public bool CanCloseDevice
        {
            get
            {
                //if (_pipeline is null) return false;
                //if (_pipeline.IsOpen) return true;
                if (_openingError)
                {
                    _openingError = false;
                    return true;
                }
                return false;
            }
        }

        public void OpenDevice()
        {
            try
            {
                NotifyOfPropertyChange(() => CanOpenDevice);
                NotifyOfPropertyChange(() => CanCloseDevice);
                NotifyOfPropertyChange(() => CanStartReceiving);
                NotifyOfPropertyChange(() => CanStopReceiving);
                NotifyOfPropertyChange(() => CanStartReceiving);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);

                // Enables Close port button in case of exception, enables CanOpenDevice to return TRUE
                _openingError = true;
                NotifyOfPropertyChange(() => CanOpenDevice);
                NotifyOfPropertyChange(() => CanCloseDevice);
            }
        }

        public async Task CloseDevice()
        {
            NotifyOfPropertyChange(() => CanCloseDevice);
            NotifyOfPropertyChange(() => CanOpenDevice);
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
        }

        public bool CanStartReceiving
        {
            get
            {
                return false;
            }
        }

        public void StartReceiving()
        {

            _receivingButtonActivated = true;
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
        }

        public bool CanStopReceiving
        {
            get
            {
                return false;
            }
        }

        public void StopReceiving()
        {
            //_pipeline.Close();
            _receivingButtonActivated = false;
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
        }

        public string SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                _selectedCommand = value;
                NotifyOfPropertyChange(() => SelectedCommand);
                NotifyOfPropertyChange(() => DataInputState);
                NotifyOfPropertyChange(() => CanAddCommand);
            }
        }

        public bool DataInputState
        {
            get
            {
                return false;
            }
        }


        public string DataInput
        {
            get => _dataInput;
            set
            {
                _dataInput = value;
                ValidateCommandDataPack(_dataInput);
                NotifyOfPropertyChange(() => DataInput);
                NotifyOfPropertyChange(() => CanAddCommand);
            }
        }

        public void AddCommand()
        {
        }

        public bool CanAddCommand
        {
            get
            {
                return true;
            }
        }

        public bool CanAddCommandEventHandler()
        {
            if (CanAddCommand)
            {
                AddCommand();
                return true;
            }
            return false;
        }

       

        public bool HasErrors
        {
            get => _validationErrors.Count > 0;
        }

        private async void ValidateCommandDataPack(string data)
        {
            const string propertyKey = "DataInput";
            ICollection<string> validationErrors = null;
            /* Call service asynchronously */
            var isValid = await Task.Run(() => _dataPackValidation.ValidateDataPack(data, out validationErrors))
            .ConfigureAwait(false);

            if (!isValid)
            {
                /* Update the collection in the dictionary returned by the GetErrors method */
                _validationErrors[propertyKey] = validationErrors;
                /* Raise event to tell WPF to execute the GetErrors method */
                RaiseErrorsChanged(propertyKey);
            }
            else if (_validationErrors.ContainsKey(propertyKey))
            {
                /* Remove all errors for this property */
                _validationErrors.Remove(propertyKey);
                /* Raise event to tell WPF to execute the GetErrors method */
                RaiseErrorsChanged(propertyKey);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName))
                return null;

            return _validationErrors[propertyName];
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}