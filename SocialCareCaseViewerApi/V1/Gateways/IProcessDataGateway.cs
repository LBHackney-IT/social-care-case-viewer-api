using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IProcessDataGateway
    {
        IEnumerable<CareCaseData> GetProcessData(ListCasesRequest request);
        IEnumerable<CareCaseData> GetProcessData(long mosaicId, string officerEmail);
        Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc);
    }
}
