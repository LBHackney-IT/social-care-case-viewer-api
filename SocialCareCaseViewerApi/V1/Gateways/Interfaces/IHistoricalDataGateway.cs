using SocialCareCaseViewerApi.V1.Domain;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IHistoricalDataGateway
    {
        List<CaseNote> GetCaseNotesByPersonId(long personId);

        CaseNote? GetCaseNoteById(long caseNoteId);

        IEnumerable<Visit> GetVisitByPersonId(long personId);

        Visit? GetVisitById(long visitId);
    }
}
