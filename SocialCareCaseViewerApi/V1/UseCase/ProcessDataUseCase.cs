using System.Threading.Tasks;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class ProcessDataUseCase : IProcessDataUseCase
    {
        private readonly IProcessDataGateway _processDataGateway;

        public ProcessDataUseCase(IProcessDataGateway processDataGateway)
        {
            _processDataGateway = processDataGateway;
        }

        public CareCaseData Execute(string recordId)
        {
            return _processDataGateway.GetCaseById(recordId);
        }

        public Task<string> Execute(CreateCaseNoteRequest request)
        {
            var doc = request.ToEntity();

            return _processDataGateway.InsertCaseNoteDocument(doc);
        }
    }
}
