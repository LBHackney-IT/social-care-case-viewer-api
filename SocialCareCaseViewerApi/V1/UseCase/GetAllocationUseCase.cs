using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetAllocationUseCase : IGetAllocationUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public GetAllocationUseCase(IDatabaseGateway databaseGateway)
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
    }
}
