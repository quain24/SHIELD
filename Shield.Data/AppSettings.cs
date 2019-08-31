using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Shield.Data.CommonInterfaces;
using Shield.Data.Enums;


namespace Shield.Data
{
    public class AppSettings 
    {
        private Dictionary<SettingsType, ISettings> _settings = new Dictionary<SettingsType, ISettings>();  
        
        public void Save()
        {

        }

        public void Load(){ }

        public Dictionary<SettingsType, ISettings> Settings()
        {
            if(_settings.Count() != 0)
            {
                return _settings;
            }
            return null;
        }                     

    }
}
