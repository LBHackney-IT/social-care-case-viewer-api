using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_persons", Schema = "dbo")]
    public class Person : IAuditEntity
    {
        public List<Address> Addresses { get; set; }
        public List<PersonOtherName> OtherNames { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public List<AllocationSet> Allocations { get; set; }
        public List<WarningNote> WarningNotes { get; set; }

        [InverseProperty("Person")]
        public List<PersonalRelationship> PersonalRelationships { get; set; }

        [InverseProperty("OtherPerson")]
        public List<PersonalRelationship> InversePersonalRelationships { get; set; }

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


        [Column("full_name"), Required]
        [MaxLength(62)]
        public string FullName { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("date_of_death")]
        public DateTime? DateOfDeath { get; set; }

        [Column("full_ethnicity_code")]
        [MaxLength(33)]
        public string Ethnicity { get; set; }

        [Column("first_language")]
        [MaxLength(100)]
        public string FirstLanguage { get; set; }

        [Column("religion")]
        [MaxLength(80)]
        public string Religion { get; set; }

        [Column("email_address")]
        [MaxLength(240)]
        public string EmailAddress { get; set; }

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

        [MaxLength(1)]
        [Column("context_flag")]
        public string AgeContext { get; set; }
        [Column("from_dm_person")]
        [MaxLength(1)]
        public string DataIsFromDmPersonsBackup { get; set; }

        [Column("sccv_sexual_orientation")]
        [MaxLength(100)]
        public string SexualOrientation { get; set; }

        [Column("sccv_preferred_method_of_contact")]
        [MaxLength(100)]
        public string PreferredMethodOfContact { get; set; }

        [Column("restricted")]
        [MaxLength(1)]
        public string Restricted { get; set; }

        [Column("marked_for_deletion")]
        public bool MarkedForDeletion { get; set; }

        //audit props
        [Column("sccv_created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("sccv_created_by")]
        public string CreatedBy { get; set; }

        [Column("sccv_last_modified_at")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("sccv_last_modified_by")]
        public string LastModifiedBy { get; set; }

        [Column("pronoun")]
        [MaxLength(8)]
        public string Pronoun { get; set; }
    }
}
