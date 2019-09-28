using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;

namespace Shield.Data.Models
{
    public interface IAppSettingsModel
    {
        Dictionary<SettingsType, ISettings> Settings { get; set; }
    }
}