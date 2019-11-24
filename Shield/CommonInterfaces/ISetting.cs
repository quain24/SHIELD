using Shield.Enums;

namespace Shield.CommonInterfaces
{
    public interface ISetting
    {
        void SetDefaults();
        SettingsType Type{get; set;}
    }
}