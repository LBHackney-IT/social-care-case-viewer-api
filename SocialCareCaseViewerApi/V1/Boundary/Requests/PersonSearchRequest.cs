using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class PersonSearchRequest
    {
        [FromQuery(Name = "person_id")]
        public string? PersonId { get; set; }

        [FromQuery(Name = "first_name")]
        public string? FirstName { get; set; }

        [FromQuery(Name = "last_name")]
        public string? LastName { get; set; }

        [FromQuery(Name = "date_of_birth")]
        public string? DateOfBirth { get; set; }

        [FromQuery(Name = "postcode")]
        public string? Postcode { get; set; }

        [FromQuery(Name = "cursor")]
        public int? Cursor { get; set; }
    }
}
