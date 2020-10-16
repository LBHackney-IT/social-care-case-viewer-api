using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    [BsonIgnoreExtraElements]
    public class CareCaseData
    {
        public string FormName { get; set; }

        [JsonProperty("mosaic_id")]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string PersonId { get; set; }
        public string Title { get; set; }

        /// <example>
        /// Ciasom
        /// </example>
        public string FirstName { get; set; }
        /// <example>
        /// Tessellate
        /// </example>
        public string LastName { get; set; }
        /// <example>
        /// 2020-05-15
        /// </example>
        public string DateOfBirth { get; set; }

        [JsonProperty("worker_email")]
        public string OfficerEmail { get; set; }

        public string CaseFormUrl { get; set; }

        [JsonProperty("processData")]
        [BsonElement("processData")]
        public object CaseFormData { get; set; }
    }
}
