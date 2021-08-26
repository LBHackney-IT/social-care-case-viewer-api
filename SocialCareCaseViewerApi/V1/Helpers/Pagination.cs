namespace SocialCareCaseViewerApi.V1.Helpers

#nullable enable
{
    public class Pagination
    {
        public int Page { get; set; }
        public int Size { get; set; }

        public int Skip => (Page - 1) * Size;
    }
}
