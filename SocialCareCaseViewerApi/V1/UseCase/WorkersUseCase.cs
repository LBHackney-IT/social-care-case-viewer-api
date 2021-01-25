using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class WorkersUseCase : IWorkersUseCase
    {
        private IDatabaseGateway _databasegateway;

        public WorkersUseCase(IDatabaseGateway dataBaseGateway)
        {
            _databasegateway = dataBaseGateway;
        }

        public ListWorkersResponse ExecuteGet(ListWorkersRequest request)
        {
            var workers = _databasegateway.GetWorkers(request.TeamId, request.WorkerId);

            //get allocations
            //TODO: refactor to use db view or proper queries
            var allocationDetails = _databasegateway.GetWorkerAllocations(workers);

            return new ListWorkersResponse() { Workers = EntityFactory.ToDomain(workers, allocationDetails) };
        }
    }
}
