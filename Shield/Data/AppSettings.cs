using Shield.CommonInterfaces;
using Shield.Data.Models;
using Shield.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Shield.Data
{
    /// <summary>
    /// Contains all of the application settings, including devices, user info and others
    /// Saves to and loads from file (xml)
    /// Serves settings to every other object instance that requires some.
    /// </summary>
    public class AppSettings : IAppSettings
    {
        private const string SETTINGS_LOCATION = @".\Settings\";
        private const string FILE_NAME = @"settings.xml";

        private IAppSettingsModel _appSettingsModel;
        private bool _wasInitialized = false;
        private bool _defaultsLoaded = false;
        private object _lock = new object();

        public AppSettings(IAppSettingsModel AppSettingsModel)
        {
            _appSettingsModel = AppSettingsModel;

            lock (_lock)
            {
                if (!_wasInitialized)
                {
                    LoadFromFile();
                }
            }
        }

        /// <summary>
        /// Clears all settings from memory
        /// </summary>
        public void Flush()
        {
            _appSettingsModel.Settings.Clear();
        }

        public bool SaveToFile()
        {
            try
            {
                if (!Directory.Exists(SETTINGS_LOCATION))
                    Directory.CreateDirectory(SETTINGS_LOCATION);

                FileStream writer = new FileStream(SETTINGS_LOCATION + FILE_NAME, FileMode.Create, FileAccess.Write);
                DataContractSerializer ser = new DataContractSerializer(typeof(AppSettingsModel));
                ser.WriteObject(writer, _appSettingsModel);
                writer.Close();
                return true;
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException)
                {
                    Debug.WriteLine("ERROR: AppSettings save - 'settings' directory not found and could not be created!");
                }
                if (ex is FileNotFoundException)
                {
                    Debug.WriteLine("ERROR: AppSettings save - 'settings' file not found and / or could not be created!");
                }
                if (ex is SerializationException)
                {
                    Debug.WriteLine("ERROR: AppSettings save - serialization of settings file failed!");
                }
                if (ex is UnauthorizedAccessException)
                {
                    Debug.WriteLine("ERROR: AppSettings save - cannot access settings file - is it read only?");
                }

                return false;
            }
        }

        public bool LoadFromFile()
        {
            try
            {
                FileStream fs = new FileStream(SETTINGS_LOCATION + @"\" + FILE_NAME, FileMode.Open, FileAccess.Read);
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(AppSettingsModel));

                _appSettingsModel = (IAppSettingsModel)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                _defaultsLoaded = false;
                return _wasInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: AppSettings load - There was a problem with file loading.\nWill try to set default values instead and save them to file!");

                _appSettingsModel = new AppSettingsModel();

                IApplicationSettingsModel defaultApplicationSettings = new ApplicationSettingsModel();
                ISerialPortSettingsModel defaultSerialPortSettings = new SerialPortSettingsModel();
                IMoqPortSettingsModel defaultMoqPortSettings = new MoqPortSettingsModel();

                _appSettingsModel.Settings.Add(SettingsType.Application, defaultApplicationSettings);
                _appSettingsModel.Settings.Add(SettingsType.SerialDevice, defaultSerialPortSettings);
                _appSettingsModel.Settings.Add(SettingsType.MoqDevice, defaultMoqPortSettings);

                SaveToFile();

                _defaultsLoaded = true;
                _wasInitialized = true;

                if (ex is DirectoryNotFoundException)
                {
                    Debug.WriteLine("ERROR: AppSettings load - 'settings' directory not found and could not be created!");
                }
                if (ex is FileNotFoundException)
                {
                    Debug.WriteLine("ERROR: AppSettings load - 'settings' file not found and / or could not be created or no permissions!");
                }
                if (ex is SerializationException)
                {
                    Debug.WriteLine("ERROR: AppSettings load - serialization of settings file failed!");
                }
                if (ex is UnauthorizedAccessException)
                {
                    Debug.WriteLine("ERROR: AppSettings load - cannot access settings file - dont have permissions?");
                }
                if (ex is NullReferenceException || ex is SerializationException)
                {
                    Debug.WriteLine("ERROR: AppSettings load - Something wrong with settings file - was it manually modified or one of config classes has changed?");
                }
                else
                {
                    Debug.WriteLine("ERROR: AppSettings load - Unknown error / unhandled NULL exception");
                }

                return false;
            }
        }

        /// <summary>
        /// Adds new or replaces existing settings config for a given parameter
        /// </summary>
        /// <param name="type">What should be added or replaced?</param>
        /// <param name="settings">with what config pack?</param>
        public void Add(SettingsType type, ISettings settings)
        {
            if (_appSettingsModel.Settings.ContainsKey(type))
                _appSettingsModel.Settings.Remove(type);
            _appSettingsModel.Settings.Add(type, settings);
        }

        /// <summary>
        /// Gets every settings pack from currently loaded ones
        /// </summary>
        /// <returns>List of currently loaded settings</returns>
        public Dictionary<SettingsType, ISettings> GetAll()
        {
            return _appSettingsModel.Settings;
        }

        public bool Remove(SettingsType type)
        {
            if (_appSettingsModel.Settings.ContainsKey(type))
            {
                _appSettingsModel.Settings.Remove(type);
                return true;
            }
            return false;
        }

        public ISettings GetSettingsFor(SettingsType type)
        {
            if (_appSettingsModel.Settings.ContainsKey(type))
                return _appSettingsModel.Settings[type];
            return null;
        }

        public T GetSettingsFor<T>() where T : class, ISettings
        {
            foreach (var kvp in _appSettingsModel.Settings)
            {
                if (kvp.Value is T)
                    return (T)kvp.Value;
            }
            return default;
        }
    }
}