using System.Collections.Generic;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IProcessDataGateway
    {
        IEnumerable<CareCaseData> GetProcessData(ListCasesRequest request);
        IEnumerable<CareCaseData> GetProcessData(long mosaicId, string firstName, string lastName, string officerEmail, string caseNoteType);
        Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc);
    }
}
