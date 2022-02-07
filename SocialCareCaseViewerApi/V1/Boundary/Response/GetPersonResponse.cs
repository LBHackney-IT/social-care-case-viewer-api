using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class GetPersonResponse
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Pronoun { get; set; }

        public bool GenderAssignedAtBirth { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }

        public string Ethnicity { get; set; }

        public string FirstLanguage { get; set; }

        public string PreferredLanguage { get; set; }

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

        public List<OtherName> OtherNames { get; set; }
    }
}
