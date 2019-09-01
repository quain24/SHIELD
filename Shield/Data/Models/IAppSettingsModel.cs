using System.Collections.Generic;
using Shield.Data.CommonInterfaces;
using Shield.Enums;

namespace Shield.Data.Models
{
    public interface IAppSettingsModel
    {
        Dictionary<SettingsType, ISettings> Settings { get; set; }
    }
}