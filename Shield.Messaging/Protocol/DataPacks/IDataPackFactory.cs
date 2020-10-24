namespace Shield.Messaging.Protocol.DataPacks
{
    public interface IDataPackFactory
    {
        IDataPack CreateFrom<TType>(params TType[] data);
        IDataPack CreateFrom<TType>(TType data);
    }
}