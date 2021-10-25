using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class MashReferral
    {
        [JsonProperty("_id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public string Referrer { get; set; }

        public string RequestedSupport { get; set; }

        public Worker AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; }

        public IEnumerable<string> Clients { get; set; }

        public string ReferralDocumentURI { get; set; }

        public string Stage { get; set }

        public string InitialDecision { get; set; }
        public string ScreeningDecision { get; set; }
        public string FinalDecision { get; set; }

        public string ReferralCategory { get; set; }
    }
}
