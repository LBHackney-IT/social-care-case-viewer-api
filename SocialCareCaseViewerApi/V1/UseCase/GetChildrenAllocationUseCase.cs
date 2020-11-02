using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetChildrenAllocationUseCase : IGetChildrenAllocationUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public GetChildrenAllocationUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }
        public CfsAllocationList Execute(ListAllocationsRequest request)
        {
            return new CfsAllocationList
            { CfsAllocations = _databaseGateway.SelectCfsAllocations(request.MosaicId, request.WorkerEmail) };
        }
    }
}
