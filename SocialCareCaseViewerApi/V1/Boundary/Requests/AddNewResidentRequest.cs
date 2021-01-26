using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
        public string Gender { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }
        public long? NhsNumber { get; set; }

        [Required]
        [MaxLength(1)]
        public string AgeGroup { get; set; }
        public string Nationality { get; set; }
        public AddressDomain Address { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
    }

    public class PhoneNumber
    {
        public string Number { get; set; }
        public string Type { get; set; }
    }
}
