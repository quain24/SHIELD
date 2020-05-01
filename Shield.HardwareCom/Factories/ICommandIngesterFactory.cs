using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Helpers;

namespace Shield.HardwareCom.Factories
{
    public interface ICommandIngesterFactory
    {
        ICommandIngester GetIngesterUsing(IIdGenerator idGenerator);

        ICommandIngester GetIngetster();
    }
}