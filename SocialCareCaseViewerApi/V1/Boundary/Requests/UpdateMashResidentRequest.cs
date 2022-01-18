#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateMashResidentRequest
    {
        public long? SocialCareId { get; set; }
        public string? UpdateType { get; set; }
    }
}
