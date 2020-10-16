using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Domain
{
    [BsonIgnoreExtraElements]
    public class FormData
    {
        [JsonProperty("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RecordId { get; set; }
        [JsonProperty("formName")]
        [BsonElement("form_name")]
        public string FormName { get; set; }
        [JsonProperty("personId")]
        [BsonElement("mosaic_id")]
        public int PersonId { get; set; }
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
        [JsonProperty("formUrl")]
        [BsonElement("form_url")]
        public string CaseFormUrl { get; set; }
    }
}
