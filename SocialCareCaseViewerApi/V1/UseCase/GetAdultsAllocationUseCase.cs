using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetAdultsAllocationUseCase : IGetAdultsAllocationsUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public GetAdultsAllocationUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public AscAllocationList Execute(ListAllocationsRequest request)
        {
            return new AscAllocationList { AscAllocations = _databaseGateway.SelectAscAllocations(request.MosaicId, request.WorkerEmail) };
        }
    }
}
