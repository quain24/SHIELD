using System.Globalization;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class Int32DataPack : IDataPack
    {
        private readonly int _rawData;

        public Int32DataPack(int rawData)
        {
            _rawData = rawData;
        }

        public string GetDataInTransmittableFormat() => _rawData.ToString(CultureInfo.InvariantCulture);
    }
}