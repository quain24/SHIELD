using Shield.CommonInterfaces;
using Shield.Data.Models;
using Shield.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Shield.Data
{
    public class Settings : ISettings
    {
        private const string SETTINGS_LOCATION = @".\Settings\";
        private const string FILE_NAME = @"settings.xml";

        private ISettingsModel _settingsModel;

        public Settings(ISettingsModel settingsModel)
        {
            _settingsModel = settingsModel;
            AddMissingSettingPacks();
        }       

        /// <summary>
        /// Clears all settings from memory
        /// </summary>
        public void Flush()
        {
            _settingsModel.Settings.Clear();
        }

        public bool SaveToFile()
        {
            try
            {
                if (Directory.Exists(SETTINGS_LOCATION) == false)
                    Directory.CreateDirectory(SETTINGS_LOCATION);

                FileStream writer = new FileStream(SETTINGS_LOCATION + FILE_NAME, FileMode.Create, FileAccess.Write);
                DataContractSerializer serializer = new DataContractSerializer(_settingsModel.GetType(), KnownSettingTypes());
                serializer.WriteObject(writer, _settingsModel);
                writer.Close();
                return true;
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException)
                    Debug.WriteLine("ERROR: AppSettings save - 'settings' directory not found and could not be created!");

                if (ex is FileNotFoundException)
                    Debug.WriteLine("ERROR: AppSettings save - 'settings' file not found and / or could not be created!");

                if (ex is SerializationException)
                    Debug.WriteLine("ERROR: AppSettings save - serialization of settings file failed!");

                if (ex is UnauthorizedAccessException)
                    Debug.WriteLine("ERROR: AppSettings save - cannot access settings file - is it read only?");

                return false;
            }
        }

        public bool LoadFromFile()
        {
            try
            {
                FileStream fs = new FileStream(SETTINGS_LOCATION + @"\" + FILE_NAME, FileMode.Open, FileAccess.Read);
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(_settingsModel.GetType(), KnownSettingTypes());

                _settingsModel = (SettingsModel)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: AppSettings load - There was a problem with file loading.\nWill try to set default values instead and save them to file!");

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
        public void Add(SettingsType type, CommonInterfaces.ISetting settings)
        {
            if (_settingsModel.Settings.ContainsKey(type))
                _settingsModel.Settings.Remove(type);
            _settingsModel.Settings.Add(type, settings);
        }

        /// <summary>
        /// Gets every settings pack from currently loaded ones
        /// </summary>
        /// <returns>List of currently loaded settings</returns>
        public Dictionary<SettingsType, ISetting> GetAll()
        {
            return _settingsModel.Settings;
        }

        public bool Remove(SettingsType type)
        {
            if (_settingsModel.Settings.ContainsKey(type))
            {
                _settingsModel.Settings.Remove(type);
                return true;
            }
            return false;
        }

        public ISetting For(SettingsType type)
        {
            if (_settingsModel.Settings.ContainsKey(type))
                return _settingsModel.Settings[type];
            return null;
        }

        public T ForTypeOf<T>() where T : class, ISetting
        {
            foreach (var kvp in _settingsModel.Settings)
            {
                if (kvp.Value is T)
                    return (T)kvp.Value;
            }
            return default;
        }

        private List<Type> KnownSettingTypes()
        {
            var listOfTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                               from assemblyType in domainAssembly.GetTypes()
                               where typeof(ISetting).IsAssignableFrom(assemblyType)
                               select assemblyType).ToList();

            var output = listOfTypes
                .Where(a => !a.IsInterface)
                .ToList();

            return output;
        }

        private List<ISetting> CreateDefaultSettingPacks()
        {
            List<ISetting> defaultSettingPacks = new List<ISetting>();
            foreach (var t in KnownSettingTypes())
            {
                defaultSettingPacks.Add((ISetting)t.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()));
            }

            return defaultSettingPacks;
        }

        private void AddMissingSettingPacks()
        {
            var settings = CreateDefaultSettingPacks();

            foreach (var con in settings)
            {
                if (_settingsModel.Settings.ContainsKey(con.Type) == false)
                    _settingsModel.Settings.Add(con.Type, con);
            }
        }
    }
}