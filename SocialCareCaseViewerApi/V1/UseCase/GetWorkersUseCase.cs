using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using dbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetWorkersUseCase : IGetWorkersUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public GetWorkersUseCase(IDatabaseGateway dataBaseGateway)
        {
            _databaseGateway = dataBaseGateway;
        }

        public List<WorkerResponse> Execute(GetWorkersRequest request)
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

        private Worker? GetByWorkerId(int workerId)
        {
            if (workerId == 0)
            {
                return null;
            }
            var dbWorker = _databaseGateway.GetWorkerByWorkerId(workerId);
            return dbWorker?.ToDomain(false);
        }

        private Worker? GetByWorkerEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var dbWorker = _databaseGateway.GetWorkerByEmail(email);
            return dbWorker?.ToDomain(false);
        }

        private List<Worker>? GetByWorkerTeamId(int teamId)
        {
            if (teamId == 0)
            {
                return null;
            }

            var dbTeams = _databaseGateway.GetTeamsByTeamId(teamId);
            var dbWorkerTeams = dbTeams?.Select(team => team.WorkerTeams);


            List<Worker> domainWorkers = new List<Worker>();
            if (dbWorkerTeams != null)
            {
                foreach (var workerTeams in dbWorkerTeams)
                {
                    foreach (var workerTeam in workerTeams)
                    {
                        domainWorkers.Add(workerTeam.Worker.ToDomain(false));
                    }
                }
            }

            return domainWorkers;
        }
    }
}
