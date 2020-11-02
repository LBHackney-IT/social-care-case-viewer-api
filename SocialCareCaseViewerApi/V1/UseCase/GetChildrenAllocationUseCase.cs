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
        public CfsAllocationList Execute(string officerEmail, long mosaicId)
        {
            return new CfsAllocationList
            { CfsAllocations = _databaseGateway.SelectCfsAllocations(mosaicId) };
        }
    }
}
