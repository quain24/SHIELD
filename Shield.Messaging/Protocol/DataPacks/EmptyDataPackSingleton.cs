using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol.DataPacks
{
    public sealed class EmptyDataPackSingleton : IDataPack
    {
        private static EmptyDataPackSingleton _pack;
        public static EmptyDataPackSingleton GetInstance()
        {
            return _pack ?? (_pack = new EmptyDataPackSingleton());
        }
        private EmptyDataPackSingleton(){}
        public string GetDataInTransmittableFormat()
        {
            return string.Empty;
        }
    }
}
