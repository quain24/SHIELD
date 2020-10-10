using System;
using System.Globalization;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class DoubleDataPack : IDataPack
    {
        private readonly double _rawData;

        public DoubleDataPack(double rawData)
        {
            _rawData = rawData;
        }

        public string GetDataInTransmittableFormat() => _rawData.ToString(CultureInfo.InvariantCulture);
    }
}