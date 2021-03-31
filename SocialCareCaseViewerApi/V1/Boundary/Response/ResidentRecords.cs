using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ResidentRecords
    {
        public List<ResidentRecord> Cases { get; set; }
        public int? NextCursor { get; set; }
    }
}
