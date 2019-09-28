using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;

namespace Shield.Data
{
    public interface IAppSettings
    {
        void Add(SettingsType type, ISettings settings);

        void Flush();

        Dictionary<SettingsType, ISettings> GetAll();

        ISettings GetSettingsFor(SettingsType type);

        T GetSettingsFor<T>() where T : class, ISettings;

        bool LoadFromFile();

        bool Remove(SettingsType type);

        bool SaveToFile();
    }
}