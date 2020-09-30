using System;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class StringDataPack : IDataPack
    {
        private readonly string _rawData;

        public StringDataPack(string rawData)
        {
            _rawData = string.IsNullOrEmpty(rawData)
                ? throw new ArgumentNullException(nameof(rawData), "RawData should contain data!")
                : rawData;
        }

        public string GetDataInTransmittableFormat() => _rawData;
    }
}