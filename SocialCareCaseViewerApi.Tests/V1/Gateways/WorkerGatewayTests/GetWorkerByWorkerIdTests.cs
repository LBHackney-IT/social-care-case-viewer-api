using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.WorkerGatewayTests
{
    public class GetWorkerByWorkerIdTests : DatabaseTests
    {
        private WorkerGateway _workerGateway = null!;

        [SetUp]
        public void Setup()
        {
            _workerGateway = new WorkerGateway(DatabaseContext);
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsWorker()
        {
            var worker = TestHelpers.CreateWorker();
            SaveWorkerToDatabase(worker);

            var response = _workerGateway.GetWorkerByWorkerId(worker.Id);

            response.Should().BeEquivalentTo(worker.ToDomain(true));
        }

        [Test]
        public void GetWorkerByWorkerIdDoesNotReturnWorkerTeamsWithHistoricalRelationship()
        {
            var worker = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(id: 1, "worker-email@example.com"));

            var currentWorkerTeamRelationship = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 1, workerId: 1, teamId: 1, worker: worker));

            _ = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 2, workerId: 1, teamId: 1, worker: worker, endDate: DateTime.Now.AddDays(-50)));

            var workerTeams = new List<WorkerTeam> { currentWorkerTeamRelationship };

            _ = SaveTeamToDatabase(DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams)); //add the team only once, relationship added above

            var responseWorker = _workerGateway.GetWorkerByWorkerId(worker.Id);

            responseWorker?.Teams?.Count.Should().Be(1);
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsNullWhenIdNotPresent()
        {
            var worker = TestHelpers.CreateWorker();
            SaveWorkerToDatabase(worker);

            var response = _workerGateway.GetWorkerByWorkerId(worker.Id + 1);

            response.Should().BeNull();
        }

        private Worker SaveWorkerToDatabase(Worker worker)
        {
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();
            return worker;
        }

        private WorkerTeam SaveWorkerTeamToDatabase(WorkerTeam workerTeam)
        {
            DatabaseContext.WorkerTeams.Add(workerTeam);
            DatabaseContext.SaveChanges();
            return workerTeam;
        }

        private Team SaveTeamToDatabase(Team team)
        {
            DatabaseContext.Teams.Add(team);
            DatabaseContext.SaveChanges();
            return team;
        }
    }
}
