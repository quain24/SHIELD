using System.Text.Json;

namespace Shield.Messaging.Protocol.DataPacks
{
    public class JsonDataPack : IDataPack
    {
        private readonly string _serializedObject;

        public JsonDataPack(object toBeSerialized)
        {
            _serializedObject = Serialize(toBeSerialized);
        }

        private string Serialize(object toBeSerialized)
        {
            return JsonSerializer.Serialize(toBeSerialized, new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
        }

        public string GetDataInTransmittableFormat()
        {
            return _serializedObject;
        }
    }
}