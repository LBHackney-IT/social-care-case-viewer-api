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
            var result = _processDataGateway.GetProcessData(request);

            return new CareCaseDataList
            {
                Cases = result.ToList()
            };
        }

        public CareCaseDataList Execute(long mosaicId, string officerEmail, string firstName, string lastName, string dateOfBirth, string postcode)
        {
            var result = _processDataGateway.GetProcessData(mosaicId, officerEmail, firstName, lastName, dateOfBirth, postcode);

            return new CareCaseDataList
            {
                Cases = result.ToList()
            };
        }

        public Task<string> Execute(CaseNotesDocument caseNotesDoc)
        {
            return _processDataGateway.InsertCaseNoteDocument(caseNotesDoc);
        }
    }
}
