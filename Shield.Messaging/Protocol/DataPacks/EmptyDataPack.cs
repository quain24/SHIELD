using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class EmptyDataPack : IDataPack
    {
        public EmptyDataPack(){}
        public string GetDataInTransmittableFormat()
        {
            return string.Empty;
        }
    }
}
