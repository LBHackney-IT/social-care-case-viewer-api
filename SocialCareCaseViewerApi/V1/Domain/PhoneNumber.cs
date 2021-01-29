using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class PhoneNumber
    {
        [Required]
        public string Number { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
