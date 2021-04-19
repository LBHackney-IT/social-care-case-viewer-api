using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_addresses", Schema = "dbo")]
    public class Address : IAuditEntity
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("ref_addresses_people_id")]
        [MaxLength(9)]
        [Key]
        public long PersonAddressId { get; set; }

        //Tell EF core to use database generated value
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ref_address_id")]
        [MaxLength(9)]
        public long AddressId { get; set; }

        [Column("person_id")]
        [MaxLength(16)]
        public long? PersonId { get; set; }

        // If this is populated it means the address is historical
        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("address")]
        [MaxLength(464)]
        public string AddressLines { get; set; }

        [Column("post_code")]
        [MaxLength(16)]
        public string PostCode { get; set; }

        [Column("unique_id")]
        [MaxLength(15)]
        public long? Uprn { get; set; }

        [Column("from_dm_person")]
        [MaxLength(1)]
        public string DataIsFromDmPersonsBackup { get; set; }

        [Column("is_display_address")]
        [MaxLength(1)]
        public string IsDisplayAddress { get; set; }

        //audit props
        [Column("sccv_created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("sccv_created_by")]
        public string CreatedBy { get; set; }

        [Column("sccv_last_modified_at")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("sccv_last_modified_by")]
        public string LastModifiedBy { get; set; }
    }
}
