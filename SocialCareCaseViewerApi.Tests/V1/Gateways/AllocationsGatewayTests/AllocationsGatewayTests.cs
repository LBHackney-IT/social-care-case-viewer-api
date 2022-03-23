using AutoFixture;
using Bogus;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allocation = SocialCareCaseViewerApi.V1.Infrastructure.AllocationSet;


namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class AllocationsGatewayTests : DatabaseTests
    {
        private DatabaseGateway _classUnderTest;
        private DatabaseGateway _classUnderTestWithProcessDataGateway;
        private WorkerGateway _workerGateway;
        private TeamGateway _teamGateway;

        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Faker _faker;
        private Fixture _fixture;
        private ProcessDataGateway _processDataGateway;
        private Mock<IHistoricalDataGateway> _mockHistoricalSocialCareGateway;
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockSystemTime = new Mock<ISystemTime>();
            _classUnderTest = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object,
                _mockSystemTime.Object);
            _faker = new Faker();
            _fixture = new Fixture();
            _mockHistoricalSocialCareGateway = new Mock<IHistoricalDataGateway>();
            _processDataGateway = new ProcessDataGateway(MongoDbTestContext, _mockHistoricalSocialCareGateway.Object);
            _classUnderTestWithProcessDataGateway = new DatabaseGateway(DatabaseContext, _processDataGateway,
                _mockSystemTime.Object);
            _workerGateway = new WorkerGateway(DatabaseContext);
            _teamGateway = new TeamGateway(DatabaseContext);
        }



        [Test]
        public void SelectAllocationsByWorkerEmail()
        {
            // Create worker and teams
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);
            var workerTeam = TestHelpers.CreateWorkerTeam(worker.Id);
            worker.WorkerTeams = new List<WorkerTeam> { workerTeam };

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WorkerTeams.Add(workerTeam);
            DatabaseContext.SaveChanges();

            var (createAllocationRequest, _, allocator, resident, _) = TestHelpers.CreateAllocationRequest(workerId: worker.Id, teamId: workerTeam.TeamId);
            DatabaseContext.Workers.Add(allocator);
            DatabaseContext.Persons.Add(resident);
            DatabaseContext.SaveChanges();

            _classUnderTest.CreateAllocation(createAllocationRequest);

            var allocations = _classUnderTest.SelectAllocations(0, 0, worker.Email, 0, null);

            allocations.Count.Should().Be(1);
            allocations.Single().AllocatedWorkerTeam.Should().Be(workerTeam.Team.Name);
            allocations.Single().AllocatedWorker.Should().Be($"{worker.FirstName} {worker.LastName}");
        }

        [Test]
        public void SelectAllocationsByTeamId()
        {
            // Create worker and teams
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);
            var anotherWorker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 124);

            var workerTeam = TestHelpers.CreateWorkerTeam(worker.Id);
            var anotherWorkerTeam = TestHelpers.CreateWorkerTeam(anotherWorker.Id);

            worker.WorkerTeams = new List<WorkerTeam> { workerTeam };
            anotherWorker.WorkerTeams = new List<WorkerTeam> { anotherWorkerTeam };


            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WorkerTeams.Add(workerTeam);
            DatabaseContext.Workers.Add(anotherWorker);
            DatabaseContext.WorkerTeams.Add(anotherWorkerTeam);
            DatabaseContext.SaveChanges();

            var (createAllocationRequest, _, allocator, resident, _) = TestHelpers.CreateAllocationRequest(workerId: worker.Id, teamId: workerTeam.TeamId);
            var (createAnotherAllocationRequest, _, anotherAllocator, anotherResident, _) = TestHelpers.CreateAllocationRequest(workerId: anotherWorker.Id, teamId: anotherWorkerTeam.TeamId);

            DatabaseContext.Workers.Add(allocator);
            DatabaseContext.Persons.Add(resident);
            DatabaseContext.Workers.Add(anotherAllocator);
            DatabaseContext.Persons.Add(anotherResident);
            DatabaseContext.SaveChanges();

            _classUnderTest.CreateAllocation(createAllocationRequest);
            _classUnderTest.CreateAllocation(createAnotherAllocationRequest);


            var allocations = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, null);

            allocations.Count.Should().Be(1);
            allocations.Single().AllocatedWorkerTeam.Should().Be(workerTeam.Team.Name);
            allocations.Single().AllocatedWorker.Should().Be($"{worker.FirstName} {worker.LastName}");
        }

        [Test]
        public void SelectAllocationsByTeamIdAndStatusReturnsExpectedAllocations()
        {
            var allocation = TestHelpers.CreateAllocation(teamId: 3);
            var anotherAllocation = TestHelpers.CreateAllocation(teamId: 3);
            allocation.CaseStatus = "CLOSED";
            anotherAllocation.CaseStatus = "OPEN";
            DatabaseContext.Allocations.Add(allocation);
            DatabaseContext.Allocations.Add(anotherAllocation);
            DatabaseContext.SaveChanges();
            var allocations = _classUnderTest.SelectAllocations(0, 0, null, 3, "CLOSED");
            allocations.Count.Should().Be(1);
        }


        [Test]
        public void UpdateWorkerUpdatesTheTeamSetOnAnyAllocations()
        {
            // Create worker and teams
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);
            var differentTeam = TestHelpers.CreateTeam(name: "different team X", context: worker.ContextFlag);
            var workerTeam = TestHelpers.CreateWorkerTeam(worker.Id);
            worker.WorkerTeams = new List<WorkerTeam> { workerTeam };

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WorkerTeams.Add(workerTeam);
            DatabaseContext.Teams.Add(differentTeam);
            DatabaseContext.SaveChanges();

            // Check worker's details before updating
            var originalWorker = _workerGateway.GetWorkerByWorkerId(worker.Id);
            var currentTeam = DatabaseContext.WorkerTeams.Single(x => x.WorkerId == originalWorker.Id).Team;

            originalWorker?.Teams?.Count.Should().Be(1);
            originalWorker?.Teams?.Single().Id.Should().Be(currentTeam.Id);
            originalWorker?.AllocationCount.Should().Be(0);

            var (createAllocationRequest, _, allocator, resident, _) = TestHelpers.CreateAllocationRequest(workerId: worker.Id, teamId: workerTeam.TeamId);
            DatabaseContext.Workers.Add(allocator);
            DatabaseContext.Persons.Add(resident);
            DatabaseContext.SaveChanges();

            // Check allocations assigned to the worker
            _classUnderTest.CreateAllocation(createAllocationRequest);
            originalWorker = _workerGateway.GetWorkerByWorkerId(worker.Id);
            originalWorker?.AllocationCount.Should().Be(1);

            var allocation = _classUnderTest.SelectAllocations(0, workerId: originalWorker.Id, "", 0, null);

            allocation.Count.Should().Be(1);
            allocation.Single().AllocatedWorkerTeam.Should().Be(currentTeam.Name);

            // Update worker to be in a new team
            var updateTeamRequest = TestHelpers.CreateUpdateWorkersRequest(teamId: differentTeam.Id, teamName: differentTeam.Name,
                workerId: originalWorker.Id, firstName: originalWorker.FirstName,
                lastName: originalWorker.LastName, role: originalWorker.Role, contextFlag: originalWorker.ContextFlag);

            _classUnderTest.UpdateWorker(updateTeamRequest);

            // Check worker's details after updating
            var getUpdatedWorker = _workerGateway.GetWorkerByWorkerId(originalWorker.Id);

            getUpdatedWorker?.Teams?.Count.Should().Be(1);
            getUpdatedWorker?.Teams?.Single().Id.Should().Be(differentTeam.Id);
            getUpdatedWorker?.AllocationCount.Should().Be(1);

            // Check allocations assigned to the worker have been updated
            var updatedAllocations = _classUnderTest.SelectAllocations(0, workerId: getUpdatedWorker.Id, "", 0, null);
            updatedAllocations.Count.Should().Be(1);
            updatedAllocations.Single().AllocatedWorkerTeam.Should().Be(differentTeam.Name);
        }
        [Test]
        public void CreatingAnAllocationShouldInsertIntoTheDatabase()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations;
            var insertedRecord = query.First(x => x.Id == response.AllocationId);

            insertedRecord.PersonId.Should().Be(request.MosaicId);
            insertedRecord.WorkerId.Should().Be(worker.Id);
            insertedRecord.CreatedBy.Should().Be(createdByWorker.Email);
        }

        [Test]
        public void UpdatingAllocationShouldUpdateTheRecordInTheDatabase()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();

            var allocation = new Allocation
            {
                Id = request.Id,
                AllocationEndDate = null,
                AllocationStartDate = allocationStartDate,
                CreatedAt = allocationStartDate,
                CaseStatus = "Open",
                CaseClosureDate = null,
                LastModifiedAt = null,
                CreatedBy = deAllocatedByWorker.Email,
                LastModifiedBy = null,
                PersonId = person.Id,
                TeamId = team.Id,
                WorkerId = worker.Id
            };

            DatabaseContext.Allocations.Add(allocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
                .Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations;
            var updatedRecord = query.First(x => x.Id == allocation.Id);

            Assert.AreEqual("Closed", updatedRecord.CaseStatus);
            Assert.AreEqual(worker.Id, updatedRecord.WorkerId);
            Assert.AreEqual(team.Id, updatedRecord.TeamId);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.AllocationEndDate);
            Assert.AreEqual(updatedRecord.CreatedBy, deAllocatedByWorker.Email);
            Assert.AreEqual(updatedRecord.CreatedAt, allocation.CreatedAt);
            Assert.AreEqual(updatedRecord.LastModifiedBy, deAllocatedByWorker.Email);
        }


    }
}
