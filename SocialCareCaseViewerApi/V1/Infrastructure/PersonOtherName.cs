using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_person_other_name", Schema = "dbo")]
    public class PersonOtherName
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("person_id")]
        public long PersonId { get; set; }

        [Column("first_name")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(100)]
        public string LastName { get; set; }
    }
}
