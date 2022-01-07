using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_case_note_types", Schema = "dbo")]
    public class HistoricalNoteType
    {
        [Column("note_type")]
        [MaxLength(16)]
        [Key]
        public string? Type { get; set; }

        [Column("note_type_description")]
        [MaxLength(80)]
        public string? Description { get; set; }

        public List<HistoricalCaseNote>? CaseNotes { get; set; }
    }
}
