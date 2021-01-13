using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CreateAllocationUseCase : ICreateAllocationUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public CreateAllocationUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public CreateAllocationRequest Execute(CreateAllocationRequest request)
        {
            return _databaseGateway.CreateAllocation(request);
        }
    }
}
