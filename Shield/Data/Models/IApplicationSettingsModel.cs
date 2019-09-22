using Shield.CommonInterfaces;

namespace Shield.Data.Models
{
    public interface IApplicationSettingsModel : ISettings
    {
        int DataSize { get; set; }
        int IdSize { get; set; }
        int CommandTypeSize { get; set; }
    }
}