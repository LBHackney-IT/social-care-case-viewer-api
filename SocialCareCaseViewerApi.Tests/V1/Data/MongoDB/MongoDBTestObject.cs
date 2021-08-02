using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Data.MongoDB
{
    [BsonIgnoreExtraElements]
    public class MongoDBTestObject : CaseNoteBase
    {
        [JsonProperty("marked_for_deletion")]
        public string? MarkedForDeletion { get; set; }
    }
}
