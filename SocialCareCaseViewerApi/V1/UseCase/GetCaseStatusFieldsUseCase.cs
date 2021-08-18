using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetCaseStatusFieldsUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public GetCaseStatusFieldsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public GetCaseStatusFieldsResponse Execute(GetCaseStatusFieldsRequest request)
        {
            return new GetCaseStatusFieldsResponse()
            {
                Fields = _databaseGateway.GetCaseStatusFieldsByType(request.Type)
            };
        }
    }
}
