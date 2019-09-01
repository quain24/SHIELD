using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Data.Models
{
    [Serializable]
    public class MoqPortSettingsModel : IMoqPortSettingsModel
    {
        public int PortNumber { get; set; }
    }
}
