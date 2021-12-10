using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class ResidentInformationList
    {
        public List<ResidentInformation> Residents { get; set; }

        public string NextCursor { get; set; }

        public int TotalCount { get; set; }
    }
}
