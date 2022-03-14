using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CareCaseDataList
    {
        public List<CareCaseData> Cases { get; set; }
        public int? NextCursor { get; set; }
        public int? TotalCount { get; set; }
        public long? DeletedRecordsCount { get; set; }
    }
}
