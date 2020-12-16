using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    [BsonIgnoreExtraElements]
    public class CareCaseData
    {
        [JsonProperty("_id")]
        public string RecordId { get; set; }
        [JsonProperty("formName")]
        public string FormName { get; set; }
        [JsonProperty("personId")]
        public object PersonId { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("dateOfBirth")]
        public string DateOfBirth { get; set; }
        [JsonProperty("workerEmail")]
        public string OfficerEmail { get; set; }
        [JsonProperty("formUrl")]
        public string CaseFormUrl { get; set; }
        [JsonProperty("timestamp")]
        public string CaseFormTimestamp { get; set; }
        [JsonProperty("dateOfEvent")]
        public string DateOfEvent { get; set; }
        [JsonProperty("form_data")]
        public object CaseFormData { get; set; }
    }
}
