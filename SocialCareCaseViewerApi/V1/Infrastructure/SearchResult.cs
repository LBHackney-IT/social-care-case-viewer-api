using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class SearchResult
    {
        [Key]
        public long PersonId { get; set; }
        public int TotalRecords { get; set; }
        public float? Score { get; set; }
    }
}
