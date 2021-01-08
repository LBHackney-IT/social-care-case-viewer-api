namespace SocialCareCaseViewerApi.V1.Domain
{
    public class Worker
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? TeamId { get; set; }

        public string Role { get; set; }

    }
}
