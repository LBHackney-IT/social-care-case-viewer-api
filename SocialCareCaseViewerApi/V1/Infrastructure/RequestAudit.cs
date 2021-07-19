using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_request_audit", Schema = "dbo")]
    public class RequestAudit
    {

        [Column("id")]
        [MaxLength(16)]
        [Key]
        public int Id { get; set; }

        [Column("timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }

        [Column("action_name")]
        [MaxLength(200)]
        [Required]
        public string ActionName { get; set; }

        [Column("user_name")]
        [MaxLength(16)]
        [Required]
        public string UserName { get; set; }

        [Column("metadata", TypeName = "jsonb")]
        [Required]
        public Dictionary<string, object> Metadata { get; set; }
    }
}
