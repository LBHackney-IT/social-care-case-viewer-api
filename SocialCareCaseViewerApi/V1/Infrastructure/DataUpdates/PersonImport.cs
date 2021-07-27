using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates
{
    [Table("sccv_persons_import", Schema = "dbo")]
    public class PersonImport
    {
        [Column("person_id"), Required]
        [MaxLength(100)]
        [Key]
        public string Id { get; set; }

        [Column("ssda903_id")]
        [MaxLength(10)]
        public string SSDA903ID { get; set; }

        [Column("nhs_id")]
        [MaxLength(10)]
        public long? NhsNumber { get; set; }

        [Column("scn_id")]
        [MaxLength(9)]
        public long? ScnId { get; set; }

        [Column("upn_id")]
        [MaxLength(13)]
        public string UpnId { get; set; }

        [Column("former_upn_id")]
        [MaxLength(13)]
        public string FormerUpnId { get; set; }

        [Column("full_name"), Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        [Column("title")]
        [MaxLength(8)]
        public string Title { get; set; }

        [Column("first_name")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("date_of_death")]
        public DateTime? DateOfDeath { get; set; }

        [Column("gender")]
        [MaxLength(1)]
        public string Gender { get; set; }

        [Column("restricted")]
        [MaxLength(1)]
        public string Restricted { get; set; }

        [Column("person_id_legacy")]
        [MaxLength(16)]
        public string PersonIdLegacy { get; set; }

        [Column("full_ethnicity_code")]
        [MaxLength(33)]
        public string Ethnicity { get; set; }

        [Column("country_of_birth_code")]
        [MaxLength(16)]
        public string CountryOfBirthCode { get; set; }

        [Column("is_child_legacy")]
        [MaxLength(1)]
        public string IsChildLegacy { get; set; }

        [Column("is_adult_legacy")]
        [MaxLength(1)]
        public string IsAdultLegacy { get; set; }

        [Column("nationality")]
        [MaxLength(80)]
        public string Nationality { get; set; }

        [Column("religion")]
        [MaxLength(80)]
        public string Religion { get; set; }

        [Column("marital_status")]
        [MaxLength(80)]
        public string MaritalStatus { get; set; }

        [Column("first_language")]
        [MaxLength(100)]
        public string FirstLanguage { get; set; }

        [Column("fluency_in_english")]
        [MaxLength(100)]
        public string FluencyInEnglish { get; set; }

        [Column("email_address")]
        [MaxLength(240)]
        public string EmailAddress { get; set; }

        [MaxLength(1)]
        [Column("context_flag")]
        public string AgeContext { get; set; }

        [MaxLength(13)]
        [Column("scra_id")]
        public string ScraId { get; set; }

        [MaxLength(1)]
        [Column("interpreter_required")]
        public string InterpreterRequired { get; set; }

        [Column("from_dm_person")]
        [MaxLength(1)]
        public string DataIsFromDmPersonsBackup { get; set; }
    }
}
