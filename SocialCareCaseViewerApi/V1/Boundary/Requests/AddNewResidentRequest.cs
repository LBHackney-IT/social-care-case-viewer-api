using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class AddNewResidentRequest
    {
        public string Title { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public List<OtherName> OtherNames { get; set; }

        public string Gender { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }
        public string Ethnicity { get; set; }

        public string FirstLanguage { get; set; }

        public string Religion { get; set; }

        public string SexualOrientation { get; set; }

        public long? NhsNumber { get; set; }
        public AddressDomain Address { get; set; }

        public List<PhoneNumber> PhoneNumbers { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public string PreferredMethodOfContact { get; set; }

        [Required]
        [MaxLength(1)]
        public string ContextFlag { get; set; }

        [Required]
        [EmailAddress]
        public string CreatedBy { get; set; }
    }
}
