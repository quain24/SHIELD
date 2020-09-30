namespace Shield.Messaging.Protocol.DataPacks
{
    public class IntDataPack : IDataPack
    {
        private readonly int _rawData;

        public IntDataPack(int rawData)
        {
            _rawData = rawData;
        }

        public string GetDataInTransmittableFormat() => _rawData.ToString();
    }
}