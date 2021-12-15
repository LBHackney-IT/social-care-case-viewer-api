using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates
{
    [Table("sccv_uprn_update", Schema = "dbo")]
    public class UPRNupdate
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("address_id")]
        public long AddressId { get; set; }

        [Column("uprn")]
        public long UPRN { get; set; }
    }
}
