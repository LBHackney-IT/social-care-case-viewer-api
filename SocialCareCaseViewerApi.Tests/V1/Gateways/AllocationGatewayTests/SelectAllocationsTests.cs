using AutoFixture;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allocation = SocialCareCaseViewerApi.V1.Infrastructure.AllocationSet;
using dbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using PhoneNumberInfrastructure = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class SelectAllocationGatewayTests : DatabaseTests
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

            var (allocations, _) = _classUnderTest.SelectAllocations(0, 0, worker.Email, 0);

            allocations.Count.Should().Be(1);
            allocations.Single().AllocatedWorkerTeam.Should().Be(workerTeam.Team.Name);
            allocations.Single().AllocatedWorker.Should().Be($"{worker.FirstName} {worker.LastName}");
            allocations.Single().AllocatedWorkerTeamId.Should().Be(workerTeam.Team.Id);
        }

        [Test]
        public void SelectAllocationsByResidentIdReturnsTeamAllocationWhenWorkerAllocationIsClosed()
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

            DatabaseContext.Allocations.Where(x => x.PersonId == resident.Id && x.WorkerId != null).FirstOrDefault().CaseStatus = "closed";
            DatabaseContext.SaveChanges();

            var allRelatedAllocations = DatabaseContext.Allocations.Where(x => x.PersonId == resident.Id).ToList();

            var (allocations, _) = _classUnderTest.SelectAllocations(mosaicId: resident.Id, 0, null, 0);

            allRelatedAllocations.Count.Should().Be(2);
            allocations.Count.Should().Be(1);
            allocations.Single().AllocatedWorker.Should().BeNull();
        }

        [Test]
        public void SelectAllocationsByResidentIdReturnsOnlySingleAllocationPerTeamGivingPreferenceToTheAllocationWithATeamAssigned()
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

            var allRelatedAllocations = DatabaseContext.Allocations.Where(x => x.PersonId == resident.Id).ToList();

            var (allocations, _) = _classUnderTest.SelectAllocations(mosaicId: resident.Id, 0, null, 0);

            allRelatedAllocations.Count.Should().Be(2);
            allocations.Count.Should().Be(1);
            allocations.Single().AllocatedWorkerTeam.Should().Be(workerTeam.Team.Name);
            allocations.Single().AllocatedWorker.Should().Be($"{worker.FirstName} {worker.LastName}");
        }

        [Test]
        public void SelectAllocationsByWorkerIdReturnsOnlySingleAllocationPerTeamGivingPreferenceToTheAllocationWithATeamAssigned()
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

            var allRelatedAllocations = DatabaseContext.Allocations.Where(x => x.PersonId == resident.Id).ToList();

            var (allocations, _) = _classUnderTest.SelectAllocations(mosaicId: 0, worker.Id, null, 0);

            allRelatedAllocations.Count.Should().Be(2);
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

            var (allocations, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId);

            allocations.Count.Should().Be(2);
            allocations.FirstOrDefault().AllocatedWorker.Should().BeNull();
            allocations.LastOrDefault().AllocatedWorkerTeam.Should().Be(workerTeam.Team.Name);
            allocations.LastOrDefault().AllocatedWorker.Should().Be($"{worker.FirstName} {worker.LastName}");
        }

        [Test]
        public void SelectAllocationsByCaseStatus()
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
            DatabaseContext.Allocations.Where(x => x.TeamId == workerTeam.TeamId).FirstOrDefault().CaseStatus = "CLOSED";
            DatabaseContext.SaveChanges();

            var (allocations, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, status: "CLOSED");

            allocations.Count.Should().Be(1);
            allocations.FirstOrDefault().CaseStatus.Should().Be("CLOSED");
        }

        [Test]
        public void SelectAllocationsByTeamAllocationStatus()
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

            var (teamAllocationRequest, _, allocator, resident, _) = TestHelpers.CreateAllocationRequest(teamId: workerTeam.TeamId);
            teamAllocationRequest.AllocatedWorkerId = null;
            var (teamAndWorkerAllocationRequest, _, anotherAllocator, anotherResident, _) = TestHelpers.CreateAllocationRequest(workerId: anotherWorker.Id, teamId: workerTeam.TeamId);

            DatabaseContext.Workers.Add(allocator);
            DatabaseContext.Persons.Add(resident);
            DatabaseContext.Workers.Add(anotherAllocator);
            DatabaseContext.Persons.Add(anotherResident);
            DatabaseContext.SaveChanges();

            _classUnderTest.CreateAllocation(teamAllocationRequest);
            _classUnderTest.CreateAllocation(teamAndWorkerAllocationRequest);

            var (workerAllocations, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, teamAllocationStatus: "allocated");

            workerAllocations.Count.Should().Be(1);
            workerAllocations.FirstOrDefault().AllocatedWorker.Should().NotBeNull();

            var (teamAllocations, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, teamAllocationStatus: "unallocated");

            teamAllocations.Count.Should().Be(2);
            teamAllocations.FirstOrDefault().AllocatedWorker.Should().BeNull();
            teamAllocations.Last().AllocatedWorker.Should().BeNull();
        }
    }
}
