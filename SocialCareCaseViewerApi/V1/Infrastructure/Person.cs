using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SocialCareCaseViewerApi.V1.Domain;

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
        public List<KeyContact>? KeyContacts { get; set; }

        public List<GpDetails>? GpDetails { get; set; }

        public List<TechUse>? TechUse { get; set; }

        public List<Disability>? Disability { get; set; }

        public List<EmailAddress>? Emails { get; set; }

        public List<LastUpdated>? LastUpdated { get; set; }


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
        public string? FirstLanguage { get; set; }

        [Column("fluent_in_english")]
        public bool? FluentInEnglish { get; set; }

        [Column("interpreter_needed")]
        public bool? InterpreterNeeded { get; set; }

        [Column("communication_difficulties")]
        public bool? CommunicationDifficulties { get; set; }

        [Column("difficulty_making_decisions")]
        public bool? DifficultyMakingDecisions { get; set; }

        [Column("communication_difficulties_details")]
        public string? CommunicationDifficultiesDetails { get; set; }

        [Column("employment")]
        public string? Employment { get; set; }

        [Column("allocated_team")]
        public string? AllocatedTeam { get; set; }

        [Column("preferred_language")]
        [MaxLength(100)]
        public string? PreferredLanguage { get; set; }

        [Column("religion")]
        [MaxLength(80)]
        public string Religion { get; set; }

        [Column("email_address")]
        [MaxLength(240)]
        public string EmailAddress { get; set; }

        [Column("gender")]
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
        public string? Pronoun { get; set; }

        [Column("gender_assigned_at_birth")]
        public bool? GenderAssignedAtBirth { get; set; }

        [Column("marital_status")]
        public string? MaritalStatus { get; set; }


        [Column("immigration_status")]
        public string? ImmigrationStatus { get; set; }

        [Column("primary_support_reason")]
        public string? PrimarySupportReason { get; set; }

        [Column("care_provider")]
        public string? CareProvider { get; set; }

        [Column("living_situation")]
        public string? LivingSituation { get; set; }

        [Column("tenure_type")]
        public string? TenureType { get; set; }

        [Column("accomodation_type")]
        public string? AccomodationType { get; set; }

        [Column("access_to_home")]
        public string? AccessToHome { get; set; }

        [Column("housing_officer")]
        public string? HousingOfficer { get; set; }

        [Column("housing_staff_in_contact")]
        public bool? HousingStaffInContact { get; set; }

        [Column("cautionary_alert")]
        public bool? CautionaryAlert { get; set; }

        [Column("posession_eviction_order")]
        public string? PossessionEvictionOrder { get; set; }

        [Column("rent_record")]
        public string? RentRecord { get; set; }

        [Column("housing_benefit")]
        public string? HousingBenefit { get; set; }

        [Column("council_tenure_type")]
        public string? CouncilTenureType { get; set; }

        [Column("tenancy_household_structure")]
        public string? TenancyHouseholdStructure { get; set; }

        [Column("mental_health_section_status")]
        public string? MentalHealthSectionStatus { get; set; }

        [Column("deaf_register")]
        public string? DeafRegister { get; set; }

        [Column("blind_register")]
        public string? BlindRegister { get; set; }

        [Column("blue_badge")]
        public string? BlueBadge { get; set; }

        [Column("open_case")]
        public bool? OpenCase { get; set; }
    }
}
