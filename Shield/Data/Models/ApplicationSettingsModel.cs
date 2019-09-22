using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Data.Models
{
    /// <summary>
    /// Holds serializable applications configuration data - all of application specific options are held here:
    /// 
    /// - MessageSize - size of a single command, header included, in chars. *0001*12345678912345 - example for a message size of 20
    /// </summary>
    [Serializable]
    public class ApplicationSettingsModel : IApplicationSettingsModel
    {
        public int DataSize { get; set; }
        public int IdSize { get; set; }
        public int CommandTypeSize { get; set; }
    }
}
