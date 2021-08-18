using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetCaseStatusFieldsUseCase : IGetCaseStatusFieldsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public GetCaseStatusFieldsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public GetCaseStatusFieldsResponse Execute(GetCaseStatusFieldsRequest request)
        {
            var caseStatusTypeFields = _databaseGateway.GetCaseStatusFieldsByType(request.Type);

            return new GetCaseStatusFieldsResponse
            {
                Fields = caseStatusTypeFields.ToResponse()
            };
        }
    }
}
