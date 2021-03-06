using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Domain
{
    [BsonIgnoreExtraElements]
    public class FormData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RecordId { get; set; }
        [JsonProperty("formName")]
        [BsonElement("form_name")]
        public string FormName { get; set; }
        [JsonProperty("personId")]
        [BsonElement("mosaic_id")]
        public object PersonId { get; set; }
        [JsonProperty("firstName")]
        [BsonElement("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        [BsonElement("last_name")]
        public string LastName { get; set; }
        [JsonProperty("dateOfBirth")]
        [BsonElement("date_of_birth")]
        public string DateOfBirth { get; set; }
        [JsonProperty("workerEmail")]
        [BsonElement("worker_email")]
        public string OfficerEmail { get; set; }
        [JsonProperty("dateOfEvent")]
        [BsonElement("date_of_event")]
        public string DateOfEvent { get; set; }
        [JsonProperty("timestamp")]
        [BsonElement("timestamp")]
        public string CaseFormTimestamp { get; set; }
        [JsonProperty("formUrl")]
        [BsonElement("form_url")]
        public string CaseFormUrl { get; set; }


    }
}
