using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class EmergencyContactDetail
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string EmailAddress { get; set; }
    }
}
