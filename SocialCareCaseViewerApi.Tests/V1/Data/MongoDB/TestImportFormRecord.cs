using MongoDB.Bson.Serialization.Attributes;

namespace SocialCareCaseViewerApi.Tests.V1.Data.MongoDB
{
    [BsonIgnoreExtraElements]
    public class FormRecord
    {
        [BsonElement("timestamp")]
        public string TimeStamp { get; set; }

        [BsonElement("worker_email")]
        public string WorkerEmail { get; set; }

        [BsonElement("context_flag")]
        public string ContextFlag { get; set; }

        [BsonElement("unique_id")]
        public string UniqueId { get; set; }

        [BsonElement("form_name")]
        public string FormName { get; set; }

        [BsonElement("form_name_overall")]
        public string FormNameOverall { get; set; }

        [BsonElement("mosaic_id")]
        public string PersonId { get; set; }

        [BsonElement("first_name")]
        public string FirstName { get; set; }

        [BsonElement("last_name")]
        public string LastName { get; set; }

        [BsonElement("date_of_birth")]
        public string DateOfBirth { get; set; }

        [BsonElement("form_url")]
        public string CaseFormUrl { get; set; }

        [BsonElement("date_of_event")]
        public string DateOfEvent { get; set; }

        [BsonElement("marked_for_deletion")]
        public string MarkedForDeletion { get; set; }
    }
}
