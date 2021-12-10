using System.Linq;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class WorkerGateway : IWorkerGateway
    {
        private readonly DatabaseContext _databaseContext;

        public WorkerGateway(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Worker? GetWorkerByWorkerId(long workerId)
        {
            return _databaseContext.Workers
                .Where(x => x.Id == workerId)
                .Include(x => x.Allocations)
                .Include(x => x.WorkerTeams)
                .ThenInclude(y => y.Team)
                .FirstOrDefault()
                ?.ToDomain(true);
        }
    }
}
