using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using EmailAddress = SocialCareCaseViewerApi.V1.Domain.EmailAddress;
using KeyContact = SocialCareCaseViewerApi.V1.Domain.KeyContact;
using GpDetails = SocialCareCaseViewerApi.V1.Domain.GpDetailsDomain;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using TechUse = SocialCareCaseViewerApi.V1.Domain.TechUse;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdatePersonRequest
    {
        private string _email;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter valid person id")]
        public long Id { get; set; }

        public string Title { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public List<OtherName> OtherNames { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }
        public string Ethnicity { get; set; }

        public string FirstLanguage { get; set; }

        public string Religion { get; set; }

        public string SexualOrientation { get; set; }

        public long? NhsNumber { get; set; }
        public AddressDomain Address { get; set; }

        public List<PhoneNumber> PhoneNumbers { get; set; }

        public List<KeyContact> KeyContacts { get; set; }

        public GpDetailsDomain GpDetails { get; set; }

        public LastUpdatedDomain LastUpdated { get; set; }

        public List<String> TechUse { get; set; }

        public List<String> Disabilities { get; set; }

        public List<EmailAddress> Emails { get; set; }

        public bool? FluentInEnglish { get; set; }

        public bool? InterpreterNeeded { get; set; }

        public bool? CommunicationDifficulties { get; set; }

        public bool? DifficultyMakingDecisions { get; set; }

        public string? CommunicationDifficultiesDetails { get; set; }

        public string? Employment { get; set; }

        public string? AllocatedTeam { get; set; }

        public string? PreferredLanguage { get; set; }

        public string? Nationality { get; set; }

        public string? Pronoun { get; set; }

        public bool? GenderAssignedAtBirth { get; set; }

        public string? MaritalStatus { get; set; }

        public string? ImmigrationStatus { get; set; }

        public string? PrimarySupportReason { get; set; }

        public string? CareProvider { get; set; }

        public string? LivingSituation { get; set; }

        public string? TenureType { get; set; }

        public string? AccomodationType { get; set; }

        public string? AccessToHome { get; set; }




        [EmailAddress]
        //allow front end to send empty string for email
        public string EmailAddress
        {
            get
            {
                return _email;
            }
            set
            {
                _email = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        public string PreferredMethodOfContact { get; set; }

        [Required]
        [MaxLength(1)]
        [RegularExpression("(?i:^A|C)", ErrorMessage = "The context_flag must be 'A' or 'C' only.")]
        public string ContextFlag { get; set; }

        [Required]
        [EmailAddress]
        public string CreatedBy { get; set; }

        [Required]
        [RegularExpression("(?i:^Y|N)", ErrorMessage = "Restricted must be 'Y' or 'N' only.")]
        public string Restricted { get; set; }
    }
}
