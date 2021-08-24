using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface ICaseStatusGateway
    {
        public List<CaseStatus> GetCaseStatusesByPersonId(long personId);
    }
}
