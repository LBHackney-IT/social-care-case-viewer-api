using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("case_notes", Schema = "dbo")]
    public class HistoricalCaseNote
    {
        [Column("id")]
        [MaxLength(9)]
        [Key]
        public long Id { get; set; }

        [Column("person_id")]
        [MaxLength(9)]
        public long PersonId { get; set; }

        [Column("note_type")]
        [MaxLength(16)]
        public string? NoteType { get; set; }
        public HistoricalNoteType HistoricalNoteType { get; set; } = null!;

        [Column("title")]
        [MaxLength(100)]
        public string? Title { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        [Column("created_by")]
        [MaxLength(30)]
        public string? CreatedBy { get; set; }
        public HistoricalWorker CreatedByWorker { get; set; } = null!;

        [Column("last_updated_by")]
        [MaxLength(30)]
        public string? LastUpdatedBy { get; set; }

        [Column("note")]
        public string? Note { get; set; }
    }
}
