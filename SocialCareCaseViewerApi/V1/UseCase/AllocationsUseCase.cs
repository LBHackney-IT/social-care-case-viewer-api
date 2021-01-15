using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class AllocationsUseCase : IAllocationsUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public AllocationsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }
        public AllocationList Execute(ListAllocationsRequest request)
        {
            return new AllocationList
            {
                Allocations = _databaseGateway.SelectAllocations(request.MosaicId)
            };
        }

        public UpdateAllocationResponse ExecuteUpdate(UpdateAllocationRequest request)
        {
            return _databaseGateway.UpdateAllocation(request);
        }

        public CreateAllocationResponse ExecutePost(CreateAllocationRequest request)
        {
            return _databaseGateway.CreateAllocation(request);
        }
    }
}
