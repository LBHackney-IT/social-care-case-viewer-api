using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_persons_lookup", Schema = "dbo")]
    public class PersonIdLookup
    {
        [Column("person_id")]
        [MaxLength(100)]
        [Key]
        public string MosaicId { get; set; }

        [Column("nc_id")]
        [MaxLength(100)]
        public string NCId { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

    }
}
