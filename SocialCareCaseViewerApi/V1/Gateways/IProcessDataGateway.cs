using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IProcessDataGateway
    {
        Tuple<IEnumerable<CareCaseData>, int> GetProcessData(ListCasesRequest request, string? ncId);

        CareCaseData GetCaseById(string recordId);

        Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc);
        IOrderedEnumerable<CareCaseData> SortData(string sortBy, string orderBy, List<CareCaseData> response);
    }
}
