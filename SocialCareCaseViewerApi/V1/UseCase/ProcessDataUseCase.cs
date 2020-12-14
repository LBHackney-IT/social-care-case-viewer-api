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
        public CareCaseDataList Execute(ListCasesRequest request)
        {
            request.Limit = request.Limit < 10 ? 10 : request.Limit;
            request.Limit = request.Limit > 100 ? 100 : request.Limit;
            var result = _processDataGateway.GetProcessData(request);

            var nextCursor = result.Count() == request.Limit ? result.Max(r => Int64.Parse(r.RecordId)).ToString() : "";

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
