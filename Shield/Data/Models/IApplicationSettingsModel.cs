using Shield.CommonInterfaces;

namespace Shield.Data.Models
{
    public interface IApplicationSettingsModel : ISettings
    {
        int MessageSize { get; set; }
    }
}