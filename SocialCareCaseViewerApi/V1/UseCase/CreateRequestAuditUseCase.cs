using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CreateRequestAuditUseCase : ICreateRequestAuditUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public CreateRequestAuditUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Allow non-blocking operation on gateway exception")]
        public void Execute(CreateRequestAuditRequest request)
        {
            try
            {
                _databaseGateway.CreateRequestAudit(request);
            }
            catch (Exception)
            {
            }
        }
    }
}
