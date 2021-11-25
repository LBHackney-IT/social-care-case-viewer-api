using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Helpers
{
    public class Paginated<T>
    {
        public List<T> Items { get; set; }
        public long Count { get; set; }
    }
}
