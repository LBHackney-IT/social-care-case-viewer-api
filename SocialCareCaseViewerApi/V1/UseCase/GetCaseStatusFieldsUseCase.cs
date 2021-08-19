using System;
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
            var caseStatusType = _databaseGateway.GetCaseStatusTypeWithFields(request.Type);

            if (caseStatusType == null)
            {
                throw new CaseStatusNotFoundException();
            }

            return new GetCaseStatusFieldsResponse
            {
                Description = caseStatusType.Description,
                Name = caseStatusType.Name,
                Fields = caseStatusType?.Fields.ToResponse()
            };
        }
    }

    public class CaseStatusNotFoundException : Exception
    {
        public override string Message { get; } = "Case Status Type does not exist.";
    }
}
