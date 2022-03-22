using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.TeamGatewayTests
{
    [TestFixture]
    public class GetTeamByTeamIdTests : DatabaseTests
    {
        private TeamGateway _teamGateway = null!;

        [SetUp]
        public void Setup()
        {
            _teamGateway = new TeamGateway(DatabaseContext);
        }

        [Test]
        public void GetTeamByTeamIdReturnsTeamWithWorkers()
        {
            var team = SaveTeamToDatabase(
                DatabaseGatewayHelper.CreateTeamDatabaseEntity(new List<WorkerTeam>()));

            var response = _teamGateway.GetTeamByTeamId(team.Id);

            response.Should().BeEquivalentTo(team);
        }

        [Test]
        public void GetTeamByTeamIdReturnsNullWhenNoTeamFound()
        {
            const int nonExistentTeamId = 123;

            var response = _teamGateway.GetTeamByTeamId(nonExistentTeamId);

            response.Should().BeNull();
        }

        [Test]
        public void GetTeamByTeamIdAndGetAssociatedWorkers()
        {
            var workerOne =
                SaveWorkerToDatabase(
                    DatabaseGatewayHelper.CreateWorkerDatabaseEntity(1, "worker-one-test-email@example.com"));
            var workerTwo =
                SaveWorkerToDatabase(
                    DatabaseGatewayHelper.CreateWorkerDatabaseEntity(2, "worker-two-test-email@example.com"));
            var workerTeamOne =
                SaveWorkerTeamToDatabase(
                    DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 1, workerId: workerOne.Id,
                        worker: workerOne));
            var workerTeamTwo =
                SaveWorkerTeamToDatabase(
                    DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 2, workerId: workerTwo.Id,
                        worker: workerTwo));
            var workerTeams = new List<WorkerTeam> { workerTeamOne, workerTeamTwo };
            var team = SaveTeamToDatabase(DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams));

            var responseTeam = _teamGateway.GetTeamByTeamId(team.Id);

            responseTeam?.WorkerTeams.Count.Should().Be(2);

            var responseWorkerTeams = responseTeam?.WorkerTeams.ToList();
            var workerOneResponse =
                responseWorkerTeams?.Find(workerTeam => workerTeam.Worker.Id == workerOne.Id)?.Worker;
            var workerTwoResponse =
                responseWorkerTeams?.Find(workerTeam => workerTeam.Worker.Id == workerTwo.Id)?.Worker;

            workerOneResponse.Should().BeEquivalentTo(workerOne);
            workerTwoResponse.Should().BeEquivalentTo(workerTwo);
        }

        [Test]
        public void GetTeamByIdDoesNotReturnHistoricalWorkerTeamRelationships()
        {
            var worker = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(id: 1, "current-worker-email@example.com"));

            var currentWorkerTeamRelationship = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 1, workerId: 1, teamId: 1, worker: worker));

            var previousWorkerTeamRelationship = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 2, workerId: 1, teamId: 1, worker: worker, endDate: DateTime.Now.AddDays(-50)));

            var workerTeams = new List<WorkerTeam> { currentWorkerTeamRelationship };

            var team = SaveTeamToDatabase(DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams)); //add the team only once

            var responseTeam = _teamGateway.GetTeamByTeamId(team.Id);

            responseTeam?.WorkerTeams.Count.Should().Be(1);
        }

        private Team SaveTeamToDatabase(Team team)
        {
            DatabaseContext.Teams.Add(team);
            DatabaseContext.SaveChanges();
            return team;
        }

        private WorkerTeam SaveWorkerTeamToDatabase(WorkerTeam workerTeam)
        {
            DatabaseContext.WorkerTeams.Add(workerTeam);
            DatabaseContext.SaveChanges();
            return workerTeam;
        }

        private Worker SaveWorkerToDatabase(Worker worker)
        {
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();
            return worker;
        }
    }
}
