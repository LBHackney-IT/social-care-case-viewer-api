using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class AuditEntry
    {
        public DateTime DateTime { get; set; }
        public string TableName { get; set; }
        public string EntityState { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public Audit ToAudit()
        {
            var audit = new Audit
            {
                TableName = TableName,
                EntityState = EntityState,
                DateTime = DateTime,
                KeyValues = JsonDocument.Parse(JsonSerializer.Serialize(KeyValues)),
                OldValues = OldValues.Count == 0 ? null : JsonDocument.Parse(JsonSerializer.Serialize(OldValues)), //TODO review this JsonDocument handling
                NewValues = NewValues.Count == 0 ? null : JsonDocument.Parse(JsonSerializer.Serialize(NewValues))
            };
            return audit;
        }
    }
}
