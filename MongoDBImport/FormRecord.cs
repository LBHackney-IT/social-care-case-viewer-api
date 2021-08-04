using CsvHelper.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBImport
{
    public class FormRecord
    {
        [Name("timestamp")]
        [BsonElement("timestamp")]
        public string TimeStamp { get; set; }

        [Name("worker_email")]
        [BsonElement("worker_email")]
        public string WorkerEmail { get; set; }

        [Name("context_flag")]
        [BsonElement("context_flag")]
        public string ContextFlag { get; set; }

        [Name("unique_id")]
        [BsonElement("unique_id")]
        public string UniqueId { get; set; }

        [Name("form_name")]
        [BsonElement("form_name")]
        public string FormName { get; set; }

        [Name("form_name_overall")]
        [BsonElement("form_name_overall")]
        public string FormNameOverall { get; set; }

        [Name("mosaic_id")]
        [BsonElement("mosaic_id")]
        public string PersonId { get; set; }

        [Name("first_name")]
        [BsonElement("first_name")]
        public string FirstName { get; set; }

        [Name("last_name")]
        [BsonElement("last_name")]
        public string LastName { get; set; }

        [Name("date_of_birth")]
        [BsonElement("date_of_birth")]
        public string DateOfBirth { get; set; }

        [Name("form_url")]
        [BsonElement("form_url")]
        public string CaseFormUrl { get; set; }

        [Name("date_of_event")]
        [BsonElement("date_of_event")]
        public string DateOfEvent { get; set; }

        //[Name("marked_for_deletion")]
        //[BsonElement("marked_for_deletion")]
        //public string MarkedForDeletion { get; set; }
    }
}
