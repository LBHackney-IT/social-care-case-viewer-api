using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class GetPersonResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string? Pronoun { get; set; }

        public bool? GenderAssignedAtBirth { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }

        public string Ethnicity { get; set; }

        public string FirstLanguage { get; set; }

        public string? PreferredLanguage { get; set; }

        public bool? FluentInEnglish { get; set; }

        public bool? InterpreterNeeded { get; set; }

        public bool? CommunicationDifficulties { get; set; }

        public bool? DifficultyMakingDecisions { get; set; }

        public string? CommunicationDifficultiesDetails { get; set; }


        public string? Employment { get; set; }


        public string? MaritalStatus { get; set; }

        public string? ImmigrationStatus { get; set; }        

        public string? PrimarySupportReason { get; set; }
        public string? CareProvider { get; set; }       
        public string? LivingSituation { get; set; }    

        public string? TenureType { get; set; }   
        public string? AccomodationType { get; set; }   
        public string? AccessToHome { get; set; }       
        public string? HousingOfficer { get; set; }         
        public string Religion { get; set; }

        public string SexualOrientation { get; set; }

        public long? NhsNumber { get; set; }

        public string EmailAddress { get; set; }

        public string PreferredMethodOfContact { get; set; }

        public string ContextFlag { get; set; }

        public string CreatedBy { get; set; }
        public string Restricted { get; set; }

        public AddressDomain Address { get; set; }

        public List<PhoneNumber> PhoneNumbers { get; set; }

        public List<KeyContact>? KeyContacts { get; set; }

        public List<OtherName> OtherNames { get; set; }
    }
}
