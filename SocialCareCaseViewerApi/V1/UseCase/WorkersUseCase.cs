using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using dbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

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
            List<dbWorker> workers = new List<dbWorker>();
            List<Worker> domainWorkers = new List<Worker>();

            if (request.WorkerId != 0)
            {
                dbWorker dbWorker = _databasegateway.GetWorker(request.WorkerId);

                if (dbWorker == null) throw new WorkerNotFoundException("Worker not found");
                workers.Add(dbWorker);
                domainWorkers = EntityFactory.ToDomain(workers, true);
            }
            if (request.TeamId != 0)
            {
                var teams = _databasegateway.GetWorkersByTeamId(request.TeamId).ToList();

                if (teams.Count == 0) throw new TeamNotFoundException("Team not found");

                foreach (var t in teams)
                {
                    foreach (var w in t.WorkerTeams)
                    {
                        workers.Add(w.Worker);
                    }
                }

                domainWorkers = EntityFactory.ToDomain(workers, false);
            }

            return new ListWorkersResponse() { Workers = domainWorkers };
        }
    }
}
