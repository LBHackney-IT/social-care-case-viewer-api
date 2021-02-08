using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_audit", Schema = "dbo")]
    public class Audit
    {
        [Key]
        [Required]
        [Column("id")]
        public int Id { get; set; }

        [Column("table_name")]
        [Required]
        public string TableName { get; set; }

        [Column("entity_state")]
        [Required]
        public string EntityState { get; set; }

        [Column("date_time")]
        [Required]
        public DateTime DateTime { get; set; }

        [Column("key_values", TypeName = "jsonb")]
        public JsonDocument KeyValues { get; set; }

        [Column("old_values", TypeName = "jsonb")]
        public JsonDocument OldValues { get; set; }

        [Column("new_values", TypeName = "jsonb")]
        public JsonDocument NewValues { get; set; }
    }
}
