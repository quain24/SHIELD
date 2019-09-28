using Shield.CommonInterfaces;

namespace Shield.Data.Models
{
    public interface IApplicationSettingsModel : ISettings
    {
        int DataSize { get; set; }
        int IdSize { get; set; }
        int CommandTypeSize { get; set; }
        char Separator { get; set; }
        char Filler { get; set; }
    }
}