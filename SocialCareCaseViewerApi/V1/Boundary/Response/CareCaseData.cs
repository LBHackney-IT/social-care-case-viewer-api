using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    [BsonIgnoreExtraElements]
    public class CareCaseData
    {
        [JsonProperty("_id")]
        public string RecordId { get; set; } = null!;
        [JsonProperty("formName")]
        public string FormName { get; set; } = null!;
        [JsonProperty("personId")]
        public object PersonId { get; set; } = null!;
        [JsonProperty("firstName")]
        public string FirstName { get; set; } = null!;
        [JsonProperty("lastName")]
        public string LastName { get; set; } = null!;
        [JsonProperty("dateOfBirth")]
        public string? DateOfBirth { get; set; }
        [JsonProperty("workerEmail")]
        public string OfficerEmail { get; set; } = null!;
        [JsonProperty("formUrl")]
        public string CaseFormUrl { get; set; } = null!;
        [JsonProperty("timestamp")]
        public string CaseFormTimestamp { get; set; } = null!;
        [JsonProperty("dateOfEvent")]
        public string DateOfEvent { get; set; } = null!;
        [JsonProperty("form_data")]
        public object CaseFormData { get; set; } = null!;
        [JsonProperty("formType")]
        public string? FormType { get; set; }
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; } = false;

        [JsonProperty("deletionDetails")]
        public DeletionDetails? DeletionDetails { get; set; }
    }
}
