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
        //new props

        [Name("form_name")]
        [BsonElement("form_name")]
        public string FormName { get; set; }

        [Name("mosaic_id")]
        [BsonElement("mosaic_id")]
        public object PersonId { get; set; }

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
    }
}
