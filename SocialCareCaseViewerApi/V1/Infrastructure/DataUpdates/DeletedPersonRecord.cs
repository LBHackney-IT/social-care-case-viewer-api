using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates
{
    [Table("sccv_deleted_person_record", Schema = "dbo")]
    public class DeletedPersonRecord
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        //id of the person marked for deletion
        [Column("deleted_id")]
        [MaxLength(16)]
        public long DeletedId { get; set; }

        //person id used for merging records
        [Column("master_id")]
        [MaxLength(16)]
        public long MasterId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
