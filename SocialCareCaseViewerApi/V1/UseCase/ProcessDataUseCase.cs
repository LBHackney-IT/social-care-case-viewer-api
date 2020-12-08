using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ProcessDataUseCase : IProcessDataUseCase
    {
        private IProcessDataGateway _processDataGateway;
        public ProcessDataUseCase(IProcessDataGateway processDataGateway)
        {
            _processDataGateway = processDataGateway;
        }
        public CareCaseDataList Execute(ListCasesRequest request, int cursor, int limit)
        {
            limit = limit < 10 ? 10 : limit;
            limit = limit > 100 ? 100 : limit;
            var result = _processDataGateway.GetProcessData(cursor: cursor, limit: limit, request);

            var nextCursor = result.Count() == limit ? result.Max(r => Int64.Parse(r.RecordId)).ToString() : "";

            return new CareCaseDataList
            {
                Cases = result.ToList(),
                NextCursor = nextCursor
            };
        }

        public Task<string> Execute(CaseNotesDocument caseNotesDoc)
        {
            return _processDataGateway.InsertCaseNoteDocument(caseNotesDoc);
        }
    }
}
