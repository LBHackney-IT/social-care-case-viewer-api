using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IProcessDataGateway
    {
        Tuple<IEnumerable<ResidentRecord>, int> GetProcessData(GetRecordsRequest request, string ncId);

        ResidentRecord GetCaseById(string recordId);

        Task<string> InsertCaseNoteDocument(CaseNotesDocument caseNotesDoc);
        IOrderedEnumerable<ResidentRecord> SortData(string sortBy, string orderBy, List<ResidentRecord> response);
    }
}
