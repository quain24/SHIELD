using Shield.CommonInterfaces;
using Shield.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Shield.Persistence
{
    public class SettingsRepository
    {
        private SettingsModel _settingsModel;

        private const string SETTINGS_LOCATION = @".\Settings\";
        private const string FILE_NAME = "settings.xml";

        private void LoadData()
        { //todo work till it looks like something usable - simplify, replace single file with many maybe?
            try
            {
                var fs = new FileStream(SETTINGS_LOCATION + @"\" + FILE_NAME, FileMode.Open, FileAccess.Read);
                var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                var ser = new DataContractSerializer(_settingsModel.GetType(), KnownSettingTypes());

                _settingsModel = (SettingsModel)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
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
                    Debug.WriteLine("ERROR: AppSettings load - cannot access settings file - don't have permissions?");
                }
                if (ex is NullReferenceException || ex is SerializationException)
                {
                    Debug.WriteLine("ERROR: AppSettings load - Something wrong with settings file - was it manually modified or one of config classes has changed?");
                }
                else
                {
                    Debug.WriteLine("ERROR: AppSettings load - Unknown error / not handled NULL exception");
                }
            }
        }

        private static IEnumerable<Type> KnownSettingTypes()
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

        //public ISettings GetSettingsFor(SettingsType settingsType)
        //{
        //}
    }
}