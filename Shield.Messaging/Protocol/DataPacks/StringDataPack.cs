namespace Shield.Messaging.Protocol.DataPacks
{
    public class StringDataPack : StringArrayDataPack
    {
        public StringDataPack(string data) : base(data)
        {
        }

        public new string GetDataInTransmittableFormat() => base.GetDataInTransmittableFormat();
    }
}