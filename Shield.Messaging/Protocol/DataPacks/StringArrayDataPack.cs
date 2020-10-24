using System;
using System.Linq;
using Shield.Extensions;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class StringArrayDataPack : IDataPack
    {
        private readonly string[] _rawData;

        public StringArrayDataPack(params string[] rawData)
        {
            _rawData = rawData.IsNullOrEmpty()
                ? throw new ArgumentNullException(nameof(rawData), "RawData should contain data!")
                : rawData.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        public string GetDataInTransmittableFormat() => string.Join(GlobalConfig.DataPackSettings.DataPackSeparator, _rawData);
    }
}