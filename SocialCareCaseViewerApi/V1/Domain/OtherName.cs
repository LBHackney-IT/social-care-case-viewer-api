using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Domain
{
    //TODO: this name could be better
    public class OtherName
    {
        [Required]
        public string FirstName { get; set; }

        [Required]

        public string LastName { get; set; }
    }
}
