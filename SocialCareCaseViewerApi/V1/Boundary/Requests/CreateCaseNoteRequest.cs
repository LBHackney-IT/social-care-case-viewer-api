using System;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateCaseNoteRequest
    {
        [Required]
        public string FormName { get; set; }

        [Required]
        public string FormNameOverall { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string WorkerEmail { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int PersonId { get; set; }

        [Required]
        [RegularExpression("(?i:^A|C)", ErrorMessage = "The context_flag must be 'A' or 'C' only.")]
        public string ContextFlag { get; set; }

        [Required]
        public string CaseFormData { get; set; }
    }
}
