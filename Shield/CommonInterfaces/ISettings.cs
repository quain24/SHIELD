using Shield.Enums;
using System.Collections.Generic;

namespace Shield.CommonInterfaces
{
    public interface ISettings
    {
        void AddOrReplace(SettingsType type, ISetting settings);

        void Flush();

        ISetting For(SettingsType type);

        T ForTypeOf<T>() where T : class, ISetting;

        Dictionary<SettingsType, ISetting> GetAll();

        bool LoadFromFile();

        bool Remove(SettingsType type);

        bool SaveToFile();
    }
}