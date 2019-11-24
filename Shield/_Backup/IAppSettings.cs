using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;

namespace Shield.Data
{
    public interface IAppSettings
    {
        void Add(SettingsType type, ISetting settings);

        void Flush();

        Dictionary<SettingsType, ISetting> GetAll();

        ISetting GetSettingsFor(SettingsType type);

        T GetSettingsFor<T>() where T : class, ISetting;

        bool LoadFromFile();

        bool Remove(SettingsType type);

        bool SaveToFile();
    }
}