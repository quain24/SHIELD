using System.Collections.Generic;
using Shield.CommonInterfaces;
using Shield.Enums;

namespace Shield.Persistence.Models
{
    public interface ISettingsModel
    {
        Dictionary<SettingsType, ISetting> Settings { get; set; }
    }
}