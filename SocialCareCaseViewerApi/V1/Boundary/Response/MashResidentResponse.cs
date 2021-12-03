#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{

public class MashResidentResponse {
        public long Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Ethnicity { get; set; }
        public string? FirstLanguage { get; set; }
        public string? School { get; set; }
        public string? Address { get; set; }
        public string? Postcode { get; set; }


}

}