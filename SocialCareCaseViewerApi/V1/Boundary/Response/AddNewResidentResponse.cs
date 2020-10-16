using SocialCareCaseViewerApi.V1.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class AddNewResidentResponse
    {
        public long PersonId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? NhsNumber { get; set; }
        public string AgeGroup { get; set; }
        public string Nationality { get; set; }
        public AddressDomain Address { get; set; }
    }
}
