using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("workers", Schema = "dbo")]
    public class HistoricalWorker
    {
        [Column("id")]
        [MaxLength(9)]
        [Key]
        public long Id { get; set; }

        [Column("first_names")]
        [MaxLength(30)]
        public string? FirstNames { get; set; }

        [Column("last_names")]
        [MaxLength(30)]
        public string? LastNames { get; set; }

        [Column("system_user_id")]
        [MaxLength(64)]
        public string? SystemUserId { get; set; }

        public List<HistoricalCaseNote>? CaseNotes { get; set; }

        [Column("email_address")]
        [MaxLength(240)]
        public string? EmailAddress { get; set; }

        [Column("accessible")]
        [MaxLength(1)]
        public string? Accessible { get; set; }

        [Column("context_code")]
        [MaxLength(1)]
        public string? ContextCode { get; set; }
    }
}
