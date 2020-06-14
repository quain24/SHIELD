using Shield.CommonInterfaces;
using Shield.Enums;
using System.Collections.Generic;

namespace Shield.Persistance.Models
{
    public interface ISettingsModel
    {
        Dictionary<SettingsType, ISetting> Settings { get; set; }
    }
}