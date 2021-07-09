using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates
{
    [Table("sccv_person_record_to_be_deleted", Schema = "dbo")]
    public class PersonRecordToBeDeleted
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        //id of the person marked for deletion
        [Column("fk_person_id_to_delete")]
        [MaxLength(16)]
        public long IdToDelete { get; set; }

        //person id used for merging records
        [Column("fk_master_person_id")]
        [MaxLength(16)]
        public long MasterId { get; set; }
    }
}
