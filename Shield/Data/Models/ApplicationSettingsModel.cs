using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Data.Models
{
    [Serializable]
    public class ApplicationSettingsModel : IApplicationSettingsModel
    {
        public int MessageSize { get; set; }
    }
}
