using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class ResidentHistoricRecordCaseNote : ResidentHistoricRecord
    {
        public CaseNote CaseNote { get; set; } = null!;
    }
}
