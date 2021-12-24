using SocialCareCaseViewerApi.V1.Domain;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IHistoricalSocialCareGateway
    {
        List<CaseNote> GetAllCaseNotes(long personId);

        CaseNote? GetCaseNoteInformationById(long caseNoteId);

        IEnumerable<Visit> GetVisitInformationByPersonId(long personId);

        Visit? GetVisitInformationByVisitId(long visitId);
    }
}
