namespace SocialCareCaseViewerApi.V1.Helpers
{
    public class QueryOffset
    {
        public static int? GetNextOffset(int currentOffset, int totalRecords, int limit)
        {
            int nextOffset = currentOffset + limit;
            if (nextOffset < totalRecords)
            {
                return nextOffset;
            }
            else
            {
                return null;
            }
        }
    }
}
