using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IProcessDataGateway
    {
        IEnumerable<CareCaseData> GetProcessData(int cursor, int limit, ListCasesRequest request);
        Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc);
    }
}
