using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class WorkersUseCase : IWorkersUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IWorkerGateway _workerGateway;

        public WorkersUseCase(IDatabaseGateway databaseGateway, IWorkerGateway workerGateway)
        {
            _databaseGateway = databaseGateway;
            _workerGateway = workerGateway;
        }

        public List<WorkerResponse> ExecuteGet(GetWorkersRequest request)
        {
            List<Worker> domainWorkers = new List<Worker>();

            var workerById = GetByWorkerId(request.WorkerId);
            if (workerById != null)
            {
                domainWorkers.Add(workerById);
            }

            var workerByEmail = GetByWorkerEmail(request.Email);
            if (workerByEmail != null)
            {
                domainWorkers.Add(workerByEmail);
            }

            var workersByTeamId = GetByWorkerTeamId(request.TeamId);
            if (workersByTeamId != null)
            {
                domainWorkers.AddRange(workersByTeamId);
            }

            var distinctWorkers = domainWorkers
                .GroupBy(worker => worker.Id)
                .Select(worker => worker.First());

            return distinctWorkers.Select(worker => worker.ToResponse()).ToList();
        }

        public WorkerResponse ExecutePost(CreateWorkerRequest createWorkerRequest)
        {
            return _databaseGateway.CreateWorker(createWorkerRequest).ToDomain(true).ToResponse();
        }

        public void ExecutePatch(UpdateWorkerRequest updateWorkerRequest)
        {
            _databaseGateway.UpdateWorker(updateWorkerRequest);
        }

        private Worker? GetByWorkerId(int workerId)
        {
            return workerId == 0 ? null : _workerGateway.GetWorkerByWorkerId(workerId);
        }

        private Worker? GetByWorkerEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var dbWorker = _databaseGateway.GetWorkerByEmail(email);
            return dbWorker?.ToDomain(true);
        }

        private List<Worker>? GetByWorkerTeamId(int teamId)
        {
            if (teamId == 0)
            {
                return null;
            }

            var dbTeam = _databaseGateway.GetTeamByTeamId(teamId);
            var dbWorkerTeams = dbTeam?.WorkerTeams;


            List<Worker> domainWorkers = new List<Worker>();
            if (dbWorkerTeams != null)
            {
                foreach (var workerTeam in dbWorkerTeams)
                {
                    domainWorkers.Add(workerTeam.Worker.ToDomain(true));
                }
            }

            return domainWorkers;
        }
    }
}
