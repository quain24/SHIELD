using System.Collections.Generic;
using Shield.CommonInterfaces;
using Shield.Enums;

namespace Shield.Data
{
    public interface ISettings
    {
        void Add(SettingsType type, ISetting settings);
        void Flush();
        ISetting For(SettingsType type);
        T ForTypeOf<T>() where T : class, ISetting;
        Dictionary<SettingsType, ISetting> GetAll();
        bool LoadFromFile();
        bool Remove(SettingsType type);
        bool SaveToFile();
    }
}