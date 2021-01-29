using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_telephone_numbers", Schema = "dbo")]
    public class PhoneNumber
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("telephone_number_id")]
        [Key]
        public int Id { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("telephone_number")]
        [MaxLength(32)]
        public string Number { get; set; } //TODO: tidy up data/migrate so it has only numbers in this field

        [Column("telephone_number_type")]
        [MaxLength(80)]
        [Required]
        public string Type { get; set; }
    }
}

