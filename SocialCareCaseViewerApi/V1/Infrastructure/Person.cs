using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_persons", Schema = "dbo")]
    public class Person
    {
        [Column("person_id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }
        [Column("title")]
        [MaxLength(8)]
        public string Title { get; set; }
        [Column("first_name")]
        [MaxLength(30)]
        public string FirstName { get; set; }
        [Column("last_name")]
        [MaxLength(30)]
        public string LastName { get; set; }
        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }
        [Column("gender")]
        [MaxLength(1)]
        public string Gender { get; set; }
        [Column("nationality")]
        [MaxLength(80)]
        public string Nationality { get; set; }
        [Column("nhs_id")]
        [MaxLength(10)]
        public long? NhsNumber { get; set; }
        [Column("person_id_legacy")]
        [MaxLength(16)]
        public string PersonIdLegacy { get; set; }
        [Column("context_flag")]
        [MaxLength(1)]
        public string AgeContext { get; set; }
        [Column("from_dm_person")]
        [MaxLength(1)]
        public string DataIsFromDmPersonsBackup { get; set; }
    }
}
