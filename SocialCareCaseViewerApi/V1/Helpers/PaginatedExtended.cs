namespace SocialCareCaseViewerApi.V1.Helpers
{
    public class PaginatedExtended<T> : Paginated<T>
    {
        public long DeletedItemsCount { get; set; }
    }
}
