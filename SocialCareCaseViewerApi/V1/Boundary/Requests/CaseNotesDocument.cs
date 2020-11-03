using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CaseNotesDocument
    {
        [BsonId]
        public string Id { get; set; }
        public string RecordId { get; set; }
        public string FormName { get; set; }
        public object PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string OfficerEmail { get; set; }
        public string CaseFormUrl { get; set; }
        public string CaseFormTimestamp { get; set; }
        [Required]
        public string CaseFormData { get; set; }

    }
}
