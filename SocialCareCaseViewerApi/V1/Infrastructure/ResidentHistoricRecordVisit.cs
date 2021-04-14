using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class ResidentHistoricRecordVisit : ResidentHistoricRecord
    {
        public Visit Visit { get; set; } = null!;
    }
}
