using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class PhoneNumber
    {
        [Required]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Invalid number")]
        public string Number { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
