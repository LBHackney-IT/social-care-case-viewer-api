namespace SocialCareCaseViewerApi.V1.Domain
{
    public class Visit
    {
        public string Id { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedByEmail { get; set; }
        public string MosaicId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
