using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class AllocationsUseCase : IAllocationsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        public AllocationsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }
        public AllocationList Execute(ListAllocationsRequest request)
        {
            var (allocations, cursor) = _databaseGateway.SelectAllocations(request.MosaicId, request.WorkerId, request.WorkerEmail, request.TeamId, request.SortBy, request.Cursor, request.Status, request.TeamAllocationStatus);

            return new AllocationList
            {
                Allocations = allocations,
                NextCursor = cursor.ToString()
            };
        }

        public UpdateAllocationResponse ExecuteUpdate(UpdateAllocationRequest request)
        {
            if (request.RagRating != null)
            {
                return _databaseGateway.UpdateRagRatingInAllocation(request);
            }
            else
            {
                return _databaseGateway.UpdateAllocation(request);
            }
        }

        public CreateAllocationResponse ExecutePost(CreateAllocationRequest request)
        {
            return _databaseGateway.CreateAllocation(request);
        }
    }
}
