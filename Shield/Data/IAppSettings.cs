using System.Collections.Generic;
using Shield.CommonInterfaces;
using Shield.Enums;

namespace Shield.Data
{
    public interface IAppSettings
    {
        void Add(SettingsType type, ISettings settings);
        void Flush();
        Dictionary<SettingsType, ISettings> GetAll();
        ISettings GetSettingsFor(SettingsType type);
        bool LoadFromFile();
        bool Remove(SettingsType type);
        bool SaveToFile();
    }
}