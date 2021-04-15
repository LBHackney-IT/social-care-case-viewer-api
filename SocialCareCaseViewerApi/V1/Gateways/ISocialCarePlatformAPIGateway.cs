using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface ISocialCarePlatformAPIGateway
    {
        ListCaseNotesResponse GetCaseNotesByPersonId(string id);

        CaseNote GetCaseNoteById(string id);

        List<Visit> GetVisitsByPersonId(string id);

        Visit GetVisitByVisitId(long id);

        List<ResidentHistoricRecord> GetHistoricCaseNotesAndVisitsByPersonId(long id);
    }
}
