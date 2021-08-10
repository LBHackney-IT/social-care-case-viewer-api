using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace SocialCareCaseViewerApi.V1.Helpers
{
    public static class FilterDefinitionExtensions
    {
        public static string RenderToJson<TDocument>(this FilterDefinition<TDocument> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<TDocument>();
            return filter.Render(documentSerializer, serializerRegistry).ToJson();
        }
    }
}
