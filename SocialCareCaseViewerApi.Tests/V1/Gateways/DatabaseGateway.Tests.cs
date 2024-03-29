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
using EmailAddress = SocialCareCaseViewerApi.V1.Domain.EmailAddress;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using KeyContact = SocialCareCaseViewerApi.V1.Domain.KeyContact;
using PhoneNumberInfrastructure = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using TechUse = SocialCareCaseViewerApi.V1.Domain.TechUse;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using MongoDB.Driver.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DatabaseGatewayTests : DatabaseTests
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
        public void GetWorkerByWorkerEmailReturnsWorker()
        {
            var worker = SaveWorkerToDatabase(TestHelpers.CreateWorker());
            var response = _classUnderTest.GetWorkerByEmail(worker.Email);

            response.Should().BeEquivalentTo(worker);
        }

        [Test]
        public void GetWorkerByWorkerEmailDoesNotReturnWorkerTeamsWithHistoricalRelationship()
        {
            var worker = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(id: 1, "worker-email@example.com"));

            var currentWorkerTeamRelationship = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 1, workerId: 1, teamId: 1, worker: worker));

            _ = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 2, workerId: 1, teamId: 1, worker: worker, endDate: DateTime.Now.AddDays(-50)));

            var workerTeams = new List<WorkerTeam> { currentWorkerTeamRelationship };

            _ = SaveTeamToDatabase(DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams)); //add the team only once, relationship added above

            var responseWorker = _classUnderTest.GetWorkerByEmail(worker.Email);

            responseWorker.WorkerTeams?.Count.Should().Be(1);
        }

        [Test]
        public void GetWorkerByWorkerEmailReturnsNullWhenEmailNotPresent()
        {
            const string workerEmail = "realEmail@example.com";
            const string nonExistentWorkerEmail = "nonExistentEmail@example.com";

            SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(email: workerEmail));
            var response = _classUnderTest.GetWorkerByEmail(nonExistentWorkerEmail);

            response.Should().BeNull();
        }

        [Test]
        public void GetWorkerByWorkerEmailCaseInsensitive()
        {
            const string workerEmail = "realemail@example.com";
            const string casedEmail = "realEmail@example.com";

            var worker = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(email: workerEmail));
            var response = _classUnderTest.GetWorkerByEmail(casedEmail);

            response.Should().BeEquivalentTo(worker);
        }

        [Test]
        public void CreateWorkerInsertsWorkerIntoDatabaseAndAddsWorkerToTeam()
        {
            var (createWorkerRequest, createdTeams) = CreateWorkerAndAddToDatabase();

            var createdWorker = _classUnderTest.CreateWorker(createWorkerRequest);

            var expectedResponse = new Worker
            {
                Id = createdWorker.Id,
                FirstName = createWorkerRequest.FirstName,
                LastName = createWorkerRequest.LastName,
                Email = createWorkerRequest.EmailAddress,
                Role = createWorkerRequest.Role,
                ContextFlag = createWorkerRequest.ContextFlag,
                CreatedBy = createWorkerRequest.CreatedBy,
                DateStart = createWorkerRequest.DateStart,
            };

            var responseWorkerTeam = createdWorker.WorkerTeams?.ToList()[0];

            createdWorker.Id.Should().Be(expectedResponse.Id);
            createdWorker.FirstName.Should().Be(expectedResponse.FirstName);
            createdWorker.LastName.Should().Be(expectedResponse.LastName);
            createdWorker.Email.Should().Be(expectedResponse.Email);
            createdWorker.Role.Should().Be(expectedResponse.Role);
            createdWorker.ContextFlag.Should().Be(expectedResponse.ContextFlag);
            createdWorker.CreatedBy.Should().Be(expectedResponse.CreatedBy);
            createdWorker.DateStart.Should().Be(expectedResponse.DateStart);
            createdWorker.LastModifiedBy.Should().Be(expectedResponse.CreatedBy);
            createdWorker.IsActive.Should().BeTrue();

            responseWorkerTeam?.TeamId.Should().Be(createdTeams[0].Id);
            responseWorkerTeam?.WorkerId.Should().Be(expectedResponse.Id);
        }

        [Test]
        public void CreateWorkerThenFindByWorkerId()
        {
            var (createWorkerRequest, createdTeams) = CreateWorkerAndAddToDatabase();

            var createdWorker = _classUnderTest.CreateWorker(createWorkerRequest);

            var workerByGetWorkerById = _workerGateway.GetWorkerByWorkerId(createdWorker.Id);

            var expectedResponse = new SocialCareCaseViewerApi.V1.Domain.Worker
            {
                Id = workerByGetWorkerById.Id,
                FirstName = workerByGetWorkerById.FirstName,
                LastName = workerByGetWorkerById.LastName,
                Email = workerByGetWorkerById.Email,
                Role = workerByGetWorkerById.Role,
                ContextFlag = workerByGetWorkerById.ContextFlag,
                CreatedBy = workerByGetWorkerById.CreatedBy,
                DateStart = workerByGetWorkerById.DateStart,
            };

            var responseWorkerTeam = workerByGetWorkerById.Teams?.ToList()[0];

            workerByGetWorkerById.Id.Should().Be(expectedResponse.Id);
            workerByGetWorkerById.FirstName.Should().Be(expectedResponse.FirstName);
            workerByGetWorkerById.LastName.Should().Be(expectedResponse.LastName);
            workerByGetWorkerById.Email.Should().Be(expectedResponse.Email);
            workerByGetWorkerById.Role.Should().Be(expectedResponse.Role);
            workerByGetWorkerById.ContextFlag.Should().Be(expectedResponse.ContextFlag);
            workerByGetWorkerById.CreatedBy.Should().Be(expectedResponse.CreatedBy);
            workerByGetWorkerById.DateStart.Should().Be(expectedResponse.DateStart);

            responseWorkerTeam?.Id.Should().Be(createdTeams[0].Id);
        }

        [Test]
        public void CreateWorkerThenFindByTeamId()
        {
            var (createWorkerRequest, createdTeams) = CreateWorkerAndAddToDatabase();

            var createdWorker = _classUnderTest.CreateWorker(createWorkerRequest);

            var workerGetByTeamId =
                _teamGateway.GetTeamByTeamId(createdTeams[0].Id)?.WorkerTeams.ToList()[0].Worker;

            var expectedResponse = new Worker
            {
                Id = createdWorker.Id,
                FirstName = workerGetByTeamId.FirstName,
                LastName = workerGetByTeamId.LastName,
                Email = workerGetByTeamId.Email,
                Role = workerGetByTeamId.Role,
                ContextFlag = workerGetByTeamId.ContextFlag,
                CreatedBy = workerGetByTeamId.CreatedBy,
                DateStart = workerGetByTeamId.DateStart,
            };

            var responseWorkerTeam = workerGetByTeamId.WorkerTeams?.ToList()[0];

            workerGetByTeamId.Id.Should().Be(expectedResponse.Id);
            workerGetByTeamId.FirstName.Should().Be(expectedResponse.FirstName);
            workerGetByTeamId.LastName.Should().Be(expectedResponse.LastName);
            workerGetByTeamId.Email.Should().Be(expectedResponse.Email);
            workerGetByTeamId.Role.Should().Be(expectedResponse.Role);
            workerGetByTeamId.ContextFlag.Should().Be(expectedResponse.ContextFlag);
            workerGetByTeamId.CreatedBy.Should().Be(expectedResponse.CreatedBy);
            workerGetByTeamId.DateStart.Should().Be(expectedResponse.DateStart);

            responseWorkerTeam?.TeamId.Should().Be(createdTeams[0].Id);
            responseWorkerTeam?.WorkerId.Should().Be(expectedResponse.Id);
        }

        [Test]
        public void CreateWorkerWithInvalidTeamIdThrowsPostWorkerException()
        {
            var (createWorkerRequest, _) = CreateWorkerAndAddToDatabase(addTeamsToDatabase: false);

            Action act = () => _classUnderTest.CreateWorker(createWorkerRequest);

            act.Should().Throw<GetTeamException>()
                .WithMessage(
                    $"Team with Name {createWorkerRequest.Teams[0].Name} and ID {createWorkerRequest.Teams[0].Id} not found");
        }

        [Test]
        public void UpdateWorkerThrowsWorkerNotFoundExceptionWhenWorkerDoesNotExist()
        {
            var worker = TestHelpers.CreateWorker();

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            var request = _fixture.Build<UpdateWorkerRequest>()
                .With(x => x.WorkerId, worker.Id + 1)
                .Create();

            Action act = () => _classUnderTest.UpdateWorker(request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with Id {request.WorkerId} not found");
        }

        [Test]
        public void UpdateWorkerUpdatesAnExistingWorkerInDatabase()
        {
            var worker = TestHelpers.CreateWorker();
            var team = TestHelpers.CreateTeam();

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(teamId: team.Id, workerId: worker.Id);
            _classUnderTest.UpdateWorker(request);

            var updatedWorker = DatabaseContext.Workers.First(w => w.Id == worker.Id);

            updatedWorker.LastModifiedBy.Should().BeEquivalentTo(request.ModifiedBy);
            updatedWorker.FirstName.Should().BeEquivalentTo(request.FirstName);
            updatedWorker.LastName.Should().BeEquivalentTo(request.LastName);
            updatedWorker.ContextFlag.Should().BeEquivalentTo(request.ContextFlag);
            updatedWorker.Role.Should().BeEquivalentTo(request.Role);
            updatedWorker.DateStart.Should().Be(request.DateStart);
            updatedWorker.DateEnd.Should().BeNull();
        }

        [Test]
        public void UpdatingWorkerWithoutProvidingTeamsDoesNotTouchWorkerTeamRelationships()
        {
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);

            var workerTeam = TestHelpers.CreateWorkerTeam(workerId: worker.Id);

            worker.WorkerTeams = new List<WorkerTeam>
            {
                workerTeam
            };

            DatabaseContext.Workers.Add(worker);

            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(createATeam: false, workerId: worker.Id);

            _classUnderTest.UpdateWorker(request);

            var updatedWorker = DatabaseContext.Workers.First(w => w.Id == worker.Id);

            updatedWorker.WorkerTeams.Count.Should().Be(1);
            updatedWorker.WorkerTeams.First().EndDate.Should().BeNull();
            updatedWorker.WorkerTeams.First().StartDate.Should().Be(workerTeam.StartDate);
            updatedWorker.WorkerTeams.First().TeamId.Should().Be(workerTeam.TeamId);
        }

        [Test]
        public void MovingWorkerToANewTeamAddsNewWorkerTeamRelationshipAndEndsTheActiveOnes()
        {
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);
            worker.WorkerTeams = new List<WorkerTeam>();

            var currentWorkerTeamOne = TestHelpers.CreateWorkerTeam(workerId: worker.Id);
            worker.WorkerTeams.Add(currentWorkerTeamOne);

            DatabaseContext.Workers.Add(worker);

            var newTeam = TestHelpers.CreateTeam();
            DatabaseContext.Teams.Add(newTeam);

            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(
                createATeam: false,
                providedTeams: new List<WorkerTeamRequest>() { new WorkerTeamRequest() { Id = newTeam.Id, Name = newTeam.Name } },
                workerId: worker.Id); ;

            _classUnderTest.UpdateWorker(request);

            var updatedWorkerTeams = DatabaseContext.WorkerTeams.Where(x => x.WorkerId == worker.Id);

            updatedWorkerTeams.Count().Should().Be(2);

            //old team with end date
            updatedWorkerTeams.First(x => x.TeamId == currentWorkerTeamOne.TeamId).EndDate.Should().BeCloseTo(DateTime.Now, 1000);

            //new team with start date
            updatedWorkerTeams.First(x => x.TeamId == newTeam.Id).StartDate.Should().BeCloseTo(DateTime.Now, 1000);
            updatedWorkerTeams.First(x => x.TeamId == newTeam.Id).EndDate.Should().BeNull();
        }

        [Test]
        public void MovingWorkerToANewTeamDoesNotUpdateExistingRelationshipsThatHaveAnEndDate()
        {
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);
            worker.WorkerTeams = new List<WorkerTeam>();

            var currentWorkerTeam = TestHelpers.CreateWorkerTeam(workerId: worker.Id);
            worker.WorkerTeams.Add(currentWorkerTeam);

            var previousTeam = TestHelpers.CreateWorkerTeam(workerId: worker.Id, endDate: DateTime.Today.AddDays(-50));
            worker.WorkerTeams.Add(previousTeam);
            var expectedEndDate = previousTeam.EndDate;

            DatabaseContext.Workers.Add(worker);

            var newTeam = TestHelpers.CreateTeam();
            DatabaseContext.Teams.Add(newTeam);

            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(
                createATeam: false,
                providedTeams: new List<WorkerTeamRequest>() {
                    new WorkerTeamRequest() { Id = newTeam.Id, Name = newTeam.Name }
                },
                workerId: worker.Id); ;

            _classUnderTest.UpdateWorker(request);

            var updatedWorkerTeams = DatabaseContext.WorkerTeams.Where(x => x.WorkerId == worker.Id);

            updatedWorkerTeams.First(x => x.TeamId == previousTeam.TeamId).EndDate.Should().Be(expectedEndDate);
        }

        [Test]
        public void MovingWorkerToADifferentTeamUpdatesTheAuditColumnValues()
        {
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);
            worker.WorkerTeams = new List<WorkerTeam>();

            var currentWorkerTeam = TestHelpers.CreateWorkerTeam(workerId: worker.Id);
            worker.WorkerTeams.Add(currentWorkerTeam);

            DatabaseContext.Workers.Add(worker);

            var newTeam = TestHelpers.CreateTeam();
            DatabaseContext.Teams.Add(newTeam);

            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(
                createATeam: false,
                providedTeams: new List<WorkerTeamRequest>() {
                    new WorkerTeamRequest() { Id = newTeam.Id, Name = newTeam.Name }
                },
                workerId: worker.Id); ;

            _classUnderTest.UpdateWorker(request);

            var updatedWorkerTeams = DatabaseContext.WorkerTeams.Where(x => x.WorkerId == worker.Id);

            updatedWorkerTeams.FirstOrDefault(x => x.TeamId == newTeam.Id && x.WorkerId == worker.Id).CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1000);
            updatedWorkerTeams.FirstOrDefault(x => x.TeamId == newTeam.Id && x.WorkerId == worker.Id).CreatedBy.Should().Be(request.ModifiedBy);

            updatedWorkerTeams.FirstOrDefault(x => x.TeamId == currentWorkerTeam.Team.Id && x.WorkerId == worker.Id).LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, 1000);
            updatedWorkerTeams.FirstOrDefault(x => x.TeamId == currentWorkerTeam.Team.Id && x.WorkerId == worker.Id).LastModifiedBy.Should().Be(request.ModifiedBy);
        }

        [Test]
        public void MovingWorkerToACurrentTeamDoesNotMakeAnyChangesToCurrentWorkerTeams()
        {
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);
            worker.WorkerTeams = new List<WorkerTeam>();

            var currentWorkerTeam = TestHelpers.CreateWorkerTeam(workerId: worker.Id);
            worker.WorkerTeams.Add(currentWorkerTeam);

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(
                createATeam: false,
                providedTeams: new List<WorkerTeamRequest>() {
                    new WorkerTeamRequest() { Id = currentWorkerTeam.TeamId, Name = currentWorkerTeam.Team.Name }
                },
                workerId: worker.Id); ;

            _classUnderTest.UpdateWorker(request);

            var updatedWorkerTeams = DatabaseContext.WorkerTeams.Where(x => x.WorkerId == worker.Id);

            updatedWorkerTeams.FirstOrDefault(x => x.TeamId == currentWorkerTeam.Team.Id && x.WorkerId == worker.Id).LastModifiedAt.Should().Be(currentWorkerTeam.LastModifiedAt);
            updatedWorkerTeams.FirstOrDefault(x => x.TeamId == currentWorkerTeam.Team.Id && x.WorkerId == worker.Id).LastModifiedBy.Should().Be(currentWorkerTeam.LastModifiedBy);
        }

        [Test]
        public void MovingWorkerToAPreviousHistoricalTeamAddsNewWorkerTeam()
        {
            //historical team = worker-team relationship created before this new feature
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);
            worker.WorkerTeams = new List<WorkerTeam>();

            var historicalWorkerTeam = TestHelpers.CreateWorkerTeam(workerId: worker.Id, startDate: null, endDate: null);
            worker.WorkerTeams.Add(historicalWorkerTeam);

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(
                createATeam: false,
                providedTeams: new List<WorkerTeamRequest>() {
                    new WorkerTeamRequest() { Id = historicalWorkerTeam.Team.Id, Name = historicalWorkerTeam.Team.Name }
                },
                workerId: worker.Id); ;

            _classUnderTest.UpdateWorker(request);

            var updatedWorkerTeams = DatabaseContext.WorkerTeams.Where(x => x.WorkerId == worker.Id).OrderByDescending(x => x.Id).ToList();

            updatedWorkerTeams.Count.Should().Be(2);

            //historical worker-team realtionship that has been closed
            updatedWorkerTeams[0].EndDate.Should().BeCloseTo(DateTime.Now, 1000);

            //new relationship added with the previous histroical team. Required to get the historical data up to date
            updatedWorkerTeams[1].StartDate.Should().BeCloseTo(DateTime.Now, 1000);
        }

        [Test]
        public void MovingWorkerToANonExistentTeamThrowsGetTeamExceptionException()
        {
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(
                createATeam: false,
                providedTeams: new List<WorkerTeamRequest>() {
                    new WorkerTeamRequest() { Id = 1, Name = "foo" }
                },
                workerId: worker.Id);

            Action act = () => _classUnderTest.UpdateWorker(request);

            act.Should().Throw<GetTeamException>().WithMessage($"Team with Name foo and ID 1 not found");
        }

        [Test]
        public void MovingWorkerToMoreThanOneTeamException()
        {
            var worker = TestHelpers.CreateWorker(hasWorkerTeams: false, hasAllocations: false);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateWorkersRequest(
                createATeam: false,
                providedTeams: new List<WorkerTeamRequest>() {
                    new WorkerTeamRequest() { Id = 1, Name = "foo" },
                    new WorkerTeamRequest() { Id = 2, Name = "foobar" }
                },
                workerId: worker.Id);

            Action act = () => _classUnderTest.UpdateWorker(request);

            act.Should().Throw<Exception>().WithMessage($"Worker can have only one team");
        }

        [Test]
        public void SelectAllocationsReturnsTotalCount()
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

            var (_, _, totalCount) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, 0);

            totalCount.Should().Be(2);
        }

        [Test]
        public void SelectAllocationsReturnsAlsoWorkerTeamId()
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

            var (allocations, _, _) = _classUnderTest.SelectAllocations(0, 0, worker.Email, 0, 0);

            allocations.Count.Should().Be(1);
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

            var (allocations, _, _) = _classUnderTest.SelectAllocations(mosaicId: resident.Id, 0, null, 0, 0);

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

            var (allocations, _, _) = _classUnderTest.SelectAllocations(mosaicId: resident.Id, 0, null, 0, 0);

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

            var (allocations, _, _) = _classUnderTest.SelectAllocations(mosaicId: 0, worker.Id, null, 0, 0);

            allRelatedAllocations.Count.Should().Be(2);
            allocations.Count.Should().Be(1);
            allocations.Single().AllocatedWorkerTeam.Should().Be(workerTeam.Team.Name);
            allocations.Single().AllocatedWorker.Should().Be($"{worker.FirstName} {worker.LastName}");
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

            var (allocations, _, _) = _classUnderTest.SelectAllocations(0, 0, worker.Email, 0, 0);

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

            var (allocations, _, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, 0);

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

            var (allocations, _, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, 0, status: "CLOSED");

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

            var (workerAllocations, _, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, 0, teamAllocationStatus: "allocated");

            workerAllocations.Count.Should().Be(1);
            workerAllocations.FirstOrDefault().AllocatedWorker.Should().NotBeNull();

        }

        [Test]
        public void GetAllocationsByAllocationIdReturnsOnlyAllocationWithProvidedId()
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

            var myAllocation = DatabaseContext.Allocations.FirstOrDefault(x => x.PersonId == createAllocationRequest.MosaicId);

            var (allocations, _, _) = _classUnderTest.SelectAllocations(0, 0, null, 0, myAllocation.Id);

            allocations.Count.Should().Be(1);
            allocations.FirstOrDefault().Id.Equals(myAllocation.Id);

        }

        [Test]
        public void GetAllocationsReturnsTeamAllocationStartDateForTeamAndWorkerAllocation()
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

            var teamAllocation = DatabaseContext.Allocations.FirstOrDefault(x => x.PersonId == createAllocationRequest.MosaicId && x.WorkerId == null && x.TeamId == createAllocationRequest.AllocatedTeamId);
            var workerAllocation = DatabaseContext.Allocations.FirstOrDefault(x => x.PersonId == createAllocationRequest.MosaicId && x.WorkerId != null && x.TeamId == createAllocationRequest.AllocatedTeamId);

            var (allocations, _, _) = _classUnderTest.SelectAllocations(0, 0, null, 0, workerAllocation.Id);

            allocations.Count.Should().Be(1);
            allocations.FirstOrDefault().TeamAllocationStartDate.Equals(teamAllocation.AllocationStartDate);

        }

        [Test]
        public void GetAllocationsByTeamIdAndUnallocatedReturnsOnlyAllocationsWithoutWorkerThatDoesntHaveWorkerAllocation()
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

            var (waitingListAllocations, _, _) = _classUnderTest.SelectAllocations(0, 0, null, workerTeam.TeamId, 0, teamAllocationStatus: "unallocated");

            waitingListAllocations.Count.Should().Be(1);
            waitingListAllocations.FirstOrDefault().AllocatedWorker.Should().BeNull();

        }

        [Test]
        public void MovingWorkerToANewTeamDoesNotUpdateAnyWorkerAllocations()
        {
            // Create worker and teams
            var worker = TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false, id: 123);
            var differentTeam = TestHelpers.CreateTeam(name: "different team X", context: worker.ContextFlag);
            var workerTeam = TestHelpers.CreateWorkerTeam(worker.Id);
            worker.WorkerTeams = new List<WorkerTeam> { workerTeam };

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Teams.Add(differentTeam);
            DatabaseContext.SaveChanges();

            // Check worker's details before updating
            var originalWorker = _workerGateway.GetWorkerByWorkerId(worker.Id);
            var currentTeam = DatabaseContext.WorkerTeams.Single(x => x.WorkerId == originalWorker.Id).Team;

            originalWorker?.AllocationCount.Should().Be(0);

            var (createAllocationRequest, _, allocator, resident, _) = TestHelpers.CreateAllocationRequest(workerId: worker.Id, teamId: workerTeam.TeamId);
            DatabaseContext.Workers.Add(allocator);
            DatabaseContext.Persons.Add(resident);
            DatabaseContext.SaveChanges();

            // Check allocations assigned to the worker
            _classUnderTest.CreateAllocation(createAllocationRequest);
            originalWorker = _workerGateway.GetWorkerByWorkerId(worker.Id);
            originalWorker?.AllocationCount.Should().Be(1);

            var (allocation, _, _) = _classUnderTest.SelectAllocations(0, workerId: originalWorker.Id, "", 0, 0);

            allocation.Count.Should().Be(1);
            allocation.Single().AllocatedWorkerTeam.Should().Be(currentTeam.Name);

            // Update worker to be in a new team
            var updateTeamRequest = TestHelpers.CreateUpdateWorkersRequest(
                teamId: differentTeam.Id,
                teamName: differentTeam.Name,
                workerId: originalWorker.Id,
                firstName: originalWorker.FirstName,
                lastName: originalWorker.LastName,
                role: originalWorker.Role,
                contextFlag: originalWorker.ContextFlag);

            _classUnderTest.UpdateWorker(updateTeamRequest);

            // Check worker's details after updating
            var getUpdatedWorker = _workerGateway.GetWorkerByWorkerId(originalWorker.Id);

            getUpdatedWorker?.AllocationCount.Should().Be(1);
        }

        [Test]
        public void CreateWorkerWithDuplicateEmailThrowsPostWorkerException()
        {
            var (createWorkerRequest, _) = CreateWorkerAndAddToDatabase();
            var (createWorkerRequestDuplicate, _) = CreateWorkerAndAddToDatabase(true, createWorkerRequest.EmailAddress);

            _classUnderTest.CreateWorker(createWorkerRequest);
            Action act = () => _classUnderTest.CreateWorker(createWorkerRequestDuplicate);

            act.Should().Throw<PostWorkerException>()
                .WithMessage($"Worker with Email {createWorkerRequestDuplicate.EmailAddress} already exists");
        }

        private (CreateWorkerRequest, List<Team>) CreateWorkerAndAddToDatabase(
            bool addTeamsToDatabase = true,
            string workerEmail = null
            )
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(email: workerEmail);

            var createdTeams = new List<Team>();

            if (addTeamsToDatabase)
            {
                foreach (var team in createWorkerRequest.Teams)
                {
                    var createdTeam = new Team { Id = team.Id, Name = team.Name, Context = "A" };
                    createdTeams.Add(createdTeam);
                    SaveTeamToDatabase(createdTeam);
                }
            }

            return (createWorkerRequest, createdTeams);
        }

        [Test]
        public void GetTeamByTeamNameReturnsTeamWithWorkers()
        {
            var team = SaveTeamToDatabase(
                DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams: new List<WorkerTeam>()));

            var response = _classUnderTest.GetTeamByTeamName(team.Name);

            response.Should().BeEquivalentTo(team);
        }

        [Test]
        public void GetTeamByTeamNameDoesNotReturnWorkersWithHistoricalRelationshipWithTheTeam()
        {
            var worker = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(id: 1, "current-worker-email@example.com"));

            var currentWorkerTeamRelationship = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 1, workerId: 1, teamId: 1, worker: worker));

            _ = SaveWorkerTeamToDatabase(
                DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 2, workerId: 1, teamId: 1, worker: worker, endDate: DateTime.Now.AddDays(-50)));

            var workerTeams = new List<WorkerTeam> { currentWorkerTeamRelationship };

            var team = SaveTeamToDatabase(DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams)); //add the team only once, relationship added above

            var responseTeam = _classUnderTest.GetTeamByTeamName(team.Name);

            responseTeam?.WorkerTeams.Count.Should().Be(1);
        }

        [Test]
        public void CreatingTeamAndWorkerAllocationShouldInsertBothTeamAndWorkerAllocationsIntoTheDatabase()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.RemoveRange(DatabaseContext.Allocations);
            DatabaseContext.SaveChanges();

            _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations.ToList();
            var insertedTeamAllocation = query.First();
            var insertedTeamAndWorkerAllocation = query.Last();

            query.Count.Should().Equals(2);
            insertedTeamAllocation.RagRating.Should().Be(request.RagRating);
            insertedTeamAllocation.Summary.Should().Be(request.Summary);
            insertedTeamAllocation.CarePackage.Should().Be(request.CarePackage);
            insertedTeamAllocation.PersonId.Should().Be(request.MosaicId);
            insertedTeamAllocation.WorkerId.Should().Be(null);
            insertedTeamAllocation.CreatedBy.Should().Be(createdByWorker.Email);

            insertedTeamAndWorkerAllocation.RagRating.Should().Be(request.RagRating);
            insertedTeamAndWorkerAllocation.Summary.Should().Be(request.Summary);
            insertedTeamAndWorkerAllocation.CarePackage.Should().Be(request.CarePackage);
            insertedTeamAndWorkerAllocation.PersonId.Should().Be(request.MosaicId);
            insertedTeamAndWorkerAllocation.WorkerId.Should().Be(request.AllocatedWorkerId);
            insertedTeamAndWorkerAllocation.CreatedBy.Should().Be(createdByWorker.Email);
        }

        [Test]
        public void CreatingTeamAllocationShouldInsertTeamAllocationIntoTheDatabase()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.RemoveRange(DatabaseContext.Allocations);
            DatabaseContext.SaveChanges();

            request.AllocatedWorkerId = null;
            _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations.ToList();
            var insertedTeamAllocation = query.First();

            query.Count.Should().Equals(1);
            insertedTeamAllocation.RagRating.Should().Be(request.RagRating);
            insertedTeamAllocation.Summary.Should().Be(request.Summary);
            insertedTeamAllocation.CarePackage.Should().Be(request.CarePackage);
            insertedTeamAllocation.PersonId.Should().Be(request.MosaicId);
            insertedTeamAllocation.WorkerId.Should().Be(null);
            insertedTeamAllocation.CreatedBy.Should().Be(createdByWorker.Email);
        }

        [Test]
        public void CreatingTeamAllocationWithNoRagRatingShouldInsertTeamAllocationIntoTheDatabase()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            request.RagRating = null;
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.RemoveRange(DatabaseContext.Allocations);
            DatabaseContext.SaveChanges();

            request.AllocatedWorkerId = null;
            _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations.ToList();
            var insertedTeamAllocation = query.First();

            query.Count.Should().Equals(1);
            insertedTeamAllocation.RagRating.Should().Be(request.RagRating);
            insertedTeamAllocation.Summary.Should().Be(request.Summary);
            insertedTeamAllocation.CarePackage.Should().Be(request.CarePackage);
            insertedTeamAllocation.PersonId.Should().Be(request.MosaicId);
            insertedTeamAllocation.WorkerId.Should().Be(null);
            insertedTeamAllocation.CreatedBy.Should().Be(createdByWorker.Email);
        }

        [Test]
        public void CreatingWorkerAllocationShouldInsertWorkerAllocationIntoTheDatabase()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.RemoveRange(DatabaseContext.Allocations);
            DatabaseContext.SaveChanges();

            var dateInThePast = new DateTime(2022, 1, 1);

            var existingTeamAllocation = new AllocationSet()
            {
                PersonId = person.Id,
                TeamId = team.Id,
                WorkerId = null,
                RagRating = "urgent",
                CreatedBy = createdByWorker.Email,
                CaseStatus = "OPEN",
                MarkedForDeletion = false,
                AllocationStartDate = dateInThePast,
            };

            DatabaseContext.Allocations.Add(existingTeamAllocation);
            DatabaseContext.SaveChanges();

            _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations.ToList();
            var insertedWorkerAllocation = query.Last();

            query.Count.Should().Equals(2);

            insertedWorkerAllocation.RagRating.Should().Be(request.RagRating);
            insertedWorkerAllocation.Summary.Should().Be(request.Summary);
            insertedWorkerAllocation.CarePackage.Should().Be(request.CarePackage);
            insertedWorkerAllocation.PersonId.Should().Be(request.MosaicId);
            insertedWorkerAllocation.WorkerId.Should().Be(request.AllocatedWorkerId);
            insertedWorkerAllocation.CreatedBy.Should().Be(createdByWorker.Email);
        }

        [Test]
        public void CreatingWorkerAllocationWithDateBeforeTeamAllocationShouldThrowAnError()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            DatabaseContext.Allocations.RemoveRange(DatabaseContext.Allocations);
            DatabaseContext.SaveChanges();

            DateTime invalidWorkerAllocationTime = new DateTime(2010, 1, 18);
            request.AllocatedTeamId = team.Id;
            request.AllocationStartDate = invalidWorkerAllocationTime;

            var existingTeamAllocation = new AllocationSet()
            {
                PersonId = person.Id,
                TeamId = team.Id,
                WorkerId = null,
                RagRating = "red",
                CreatedBy = createdByWorker.Email,
                CaseStatus = "OPEN",
                MarkedForDeletion = false,
                AllocationStartDate = DateTime.Now,
            };

            DatabaseContext.Allocations.Add(existingTeamAllocation);
            DatabaseContext.SaveChanges();

            Action act = () => _classUnderTest.CreateAllocation(request);

            act.Should().Throw<CreateAllocationException>()
                .WithMessage($"Worker Allocation date must be after Team Allocation date");
        }

        [Test]
        public void CreatingAnAllocationWithoutTeamShouldRaiseAnError()
        {
            var (request, _, createdByWorker, person, _) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            request.AllocatedWorkerId = null;

            Action act = () => _classUnderTest.CreateAllocation(request);

            act.Should().Throw<CreateAllocationException>()
                .WithMessage($"Team details cannot be found");
        }

        [Test]
        public void CreatingAnAllocationWithoutTeamAndWithoutWorkerShouldRaiseAnError()
        {
            var (request, _, _, person, _) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            request.AllocatedWorkerId = null;

            Action act = () => _classUnderTest.CreateAllocation(request);

            act.Should().Throw<CreateAllocationException>()
                .WithMessage($"Team details cannot be found");
        }

        [Test]
        public void CreatingAnAllocationWithoutWorkerShouldInsertIntoTheDatabase()
        {
            var (request, _, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            request.AllocatedWorkerId = null;

            var response = _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations;
            var insertedRecord = query.First(x => x.Id == response.AllocationId);

            insertedRecord.RagRating.Should().Be(request.RagRating);
            insertedRecord.Summary.Should().Be(request.Summary);
            insertedRecord.CarePackage.Should().Be(request.CarePackage);
            insertedRecord.PersonId.Should().Be(request.MosaicId);
            insertedRecord.WorkerId.Should().BeNull();
            insertedRecord.CreatedBy.Should().Be(createdByWorker.Email);
        }

        [Test]
        public void CreatingAnAllocationForPersonWithTeamAssignedButNotWorkerShouldInsertIntoTheDatabase()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            var allocation = TestHelpers.CreateAllocation(personId: (int) request.MosaicId, teamId: request.AllocatedTeamId);
            allocation.WorkerId = null;
            allocation.MarkedForDeletion = false;
            allocation.CaseStatus = "OPEN";
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.Allocations.Add(allocation);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations;
            var insertedRecord = query.First(x => x.Id == response.AllocationId);

            insertedRecord.RagRating.Should().Be(request.RagRating);
            insertedRecord.Summary.Should().Be(request.Summary);
            insertedRecord.CarePackage.Should().Be(request.CarePackage);
            insertedRecord.PersonId.Should().Be(request.MosaicId);
            insertedRecord.WorkerId.Should().Be(worker.Id);
            insertedRecord.RagRating.Should().Be(allocation.RagRating);
            insertedRecord.CreatedBy.Should().Be(createdByWorker.Email);
        }

        [Test]
        public void CreatingTeamAllocationForPersonWithTeamShouldRaiseAnError()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            var allocation = TestHelpers.CreateAllocation(personId: (int) request.MosaicId, teamId: request.AllocatedTeamId, workerId: createdByWorker.Id);
            request.AllocatedWorkerId = null;
            allocation.WorkerId = null;
            allocation.MarkedForDeletion = false;
            allocation.CaseStatus = "OPEN";
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.Allocations.Add(allocation);
            DatabaseContext.SaveChanges();

            Action act = () => _classUnderTest.CreateAllocation(request);

            act.Should().Throw<CreateAllocationException>()
                .WithMessage($"Person is already allocated to this team");
        }

        [Test]
        public void CreatingWorkerAllocationForPersonWithTeamAndWorkerInThisTeamAssignedShouldRaiseAnError()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            var allocation = TestHelpers.CreateAllocation(personId: (int) request.MosaicId, teamId: request.AllocatedTeamId, workerId: createdByWorker.Id);
            allocation.MarkedForDeletion = false;
            allocation.CaseStatus = "OPEN";
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.Allocations.Add(allocation);
            DatabaseContext.SaveChanges();

            Action act = () => _classUnderTest.CreateAllocation(request);

            act.Should().Throw<CreateAllocationException>()
                .WithMessage($"Person has already allocated worker in this team");
        }

        [Test]
        public void UpdatingWorkerAllocationWithRagRatingShouldUpdateTheRecordInTheDatabaseWithNewRagRatingAndAuditingStamp()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, _, person, team) = TestHelpers.CreateUpdateAllocationRequest();
            request.DeallocationDate = null;
            request.DeallocationReason = null;
            request.RagRating = "High";
            request.CreatedBy = worker.Email;
            var teamAllocation = new Allocation
            {
                Id = (int) (request.Id),
                AllocationEndDate = null,
                AllocationStartDate = allocationStartDate,
                CreatedAt = allocationStartDate,
                CaseStatus = "Open",
                CaseClosureDate = null,
                LastModifiedAt = null,
                CreatedBy = worker.Email,
                LastModifiedBy = null,
                PersonId = person.Id,
                TeamId = team.Id,
                WorkerId = null
            };

            DatabaseContext.Allocations.Add(teamAllocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
                .Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            _classUnderTest.UpdateRagRatingInAllocation(request);

            var query = DatabaseContext.Allocations.ToList();
            var updatedRecord = query.First(x => x.Id == teamAllocation.Id);

            updatedRecord.RagRating.Should().Be(request.RagRating);
            updatedRecord.LastModifiedBy.Should().Be(worker.Email);
            updatedRecord.LastModifiedAt.Should().NotBeNull();
        }

        [Test]
        public void UpdatingWorkerAllocationWithExistingTeamAllocationShouldUpdateTheRecordInTheDatabase()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();
            var teamAllocation = new Allocation
            {
                Id = (int) (request.Id + 1),
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
                WorkerId = null
            };

            var workerAllocation = new Allocation
            {
                Id = (int) request.Id,
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

            DatabaseContext.Allocations.Add(teamAllocation);
            DatabaseContext.Allocations.Add(workerAllocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
                .Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            request.DeallocationScope = "worker";
            _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations.ToList();

            query.Count.Should().Equals(2);

            var updatedRecord = query.First(x => x.Id == workerAllocation.Id);

            Assert.AreEqual("Closed", updatedRecord.CaseStatus);
            Assert.AreEqual(worker.Id, updatedRecord.WorkerId);
            Assert.AreEqual(team.Id, updatedRecord.TeamId);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.AllocationEndDate);
            Assert.AreEqual(updatedRecord.CreatedBy, deAllocatedByWorker.Email);
            Assert.AreEqual(updatedRecord.CreatedAt, workerAllocation.CreatedAt);
            Assert.AreEqual(updatedRecord.LastModifiedBy, deAllocatedByWorker.Email);
        }


        [Test]
        public void DeallocatingBothWorkerAndTeamForAllocationWithExistingTeamAllocationShouldUpdateBothTheRecordInTheDatabase()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();
            var teamAllocation = new Allocation
            {
                Id = (int) (request.Id + 1),
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
                WorkerId = null
            };

            var workerAllocation = new Allocation
            {
                Id = (int) request.Id,
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

            DatabaseContext.Allocations.Add(teamAllocation);
            DatabaseContext.Allocations.Add(workerAllocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
                .Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            request.DeallocationScope = "team";
            _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations.ToList();

            query.Count.Should().Equals(2);

            var updatedWorkerAllocation = query.First(x => x.Id == workerAllocation.Id);
            var updatedTeamAllocation = query.First(x => x.TeamId == team.Id && x.WorkerId == null);

            Assert.AreEqual("Closed", updatedWorkerAllocation.CaseStatus);
            Assert.AreEqual(worker.Id, updatedWorkerAllocation.WorkerId);
            Assert.AreEqual(team.Id, updatedWorkerAllocation.TeamId);
            Assert.AreEqual(request.DeallocationDate, updatedWorkerAllocation.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedWorkerAllocation.AllocationEndDate);
            Assert.AreEqual(updatedWorkerAllocation.CreatedBy, deAllocatedByWorker.Email);
            Assert.AreEqual(updatedWorkerAllocation.CreatedAt, workerAllocation.CreatedAt);
            Assert.AreEqual(updatedWorkerAllocation.LastModifiedBy, deAllocatedByWorker.Email);

            Assert.AreEqual("Closed", updatedTeamAllocation.CaseStatus);
            Assert.AreEqual(team.Id, updatedTeamAllocation.TeamId);
            Assert.AreEqual(request.DeallocationDate, updatedTeamAllocation.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedTeamAllocation.AllocationEndDate);
            Assert.AreEqual(updatedTeamAllocation.CreatedBy, deAllocatedByWorker.Email);
            Assert.AreEqual(updatedTeamAllocation.CreatedAt, workerAllocation.CreatedAt);
            Assert.AreEqual(updatedTeamAllocation.LastModifiedBy, deAllocatedByWorker.Email);
        }


        [Test]
        public void UpdatingWorkerAllocationWithoutExistingTeamAndWithoutTeamIdAllocationShouldUpdateTheRecordInTheDatabaseAndNotCreateTeamAllocation()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();

            var workerAllocation = new Allocation
            {
                Id = (int) request.Id,
                AllocationEndDate = null,
                AllocationStartDate = allocationStartDate,
                CreatedAt = allocationStartDate,
                CaseStatus = "Open",
                CaseClosureDate = null,
                LastModifiedAt = null,
                CreatedBy = deAllocatedByWorker.Email,
                LastModifiedBy = null,
                PersonId = person.Id,
                TeamId = null,
                WorkerId = worker.Id
            };

            DatabaseContext.Allocations.Add(workerAllocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
                .Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations.ToList();

            query.Count.Should().Equals(1);

            var updatedRecord = query.First(x => x.Id == workerAllocation.Id);

            Assert.AreEqual("Closed", updatedRecord.CaseStatus);
            Assert.AreEqual(worker.Id, updatedRecord.WorkerId);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.AllocationEndDate);
            Assert.AreEqual(deAllocatedByWorker.Email, updatedRecord.CreatedBy);
            Assert.AreEqual(workerAllocation.CreatedAt, updatedRecord.CreatedAt);
        }

        [Test]
        public void UpdatingWorkerAllocationWithoutExistingTeamAllocationShouldUpdateTheRecordInTheDatabaseAndCreateTeamAllocation()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();

            var workerAllocation = new Allocation
            {
                Id = (int) request.Id,
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

            DatabaseContext.Allocations.Add(workerAllocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
                .Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations.ToList();

            query.Count.Should().Equals(2);

            var updatedRecord = query.First(x => x.Id == workerAllocation.Id);
            var teamAllocation = query.First(x => x.TeamId == workerAllocation.TeamId && x.PersonId == workerAllocation.PersonId);

            Assert.AreEqual("Closed", updatedRecord.CaseStatus);
            Assert.AreEqual(worker.Id, updatedRecord.WorkerId);
            Assert.AreEqual(team.Id, updatedRecord.TeamId);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.AllocationEndDate);
            Assert.AreEqual(deAllocatedByWorker.Email, updatedRecord.CreatedBy);
            Assert.AreEqual(workerAllocation.CreatedAt, updatedRecord.CreatedAt);

            Assert.AreEqual("Open", teamAllocation.CaseStatus);
            Assert.AreEqual(null, teamAllocation.WorkerId);
            Assert.AreEqual(workerAllocation.TeamId, teamAllocation.TeamId);
            Assert.AreEqual(workerAllocation.RagRating, teamAllocation.RagRating);
            Assert.AreEqual(workerAllocation.CreatedBy, teamAllocation.CreatedBy);
        }

        [Test]
        public void UpdatingTeamAllocationWithoutWorkerAllocationShouldUpdateTheRecordInTheDatabaseAndNotCreateAnyAdditionalAllocations()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();

            var workerAllocation = new Allocation
            {
                Id = (int) request.Id,
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
            };

            DatabaseContext.Allocations.Add(workerAllocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>()))
                .Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            request.DeallocationScope = "worker";
            _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations.ToList();

            query.Count.Should().Equals(1);

            var updatedRecord = query.First(x => x.Id == workerAllocation.Id);

            Assert.AreEqual("Closed", updatedRecord.CaseStatus);
            Assert.AreEqual(team.Id, updatedRecord.TeamId);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.AllocationEndDate);
            Assert.AreEqual(updatedRecord.CreatedBy, deAllocatedByWorker.Email);
            Assert.AreEqual(updatedRecord.CreatedAt, workerAllocation.CreatedAt);
        }

        [Test]
        public void DeallocatingTeamWhenWorkerIsNotAllocatedShouldCreateOnlyTeamDeallocatedCaseNote()
        {
            var allocationStartDate = DateTime.Now.AddDays(-10);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();

            var teamAllocation = new Allocation
            {
                Id = (int) request.Id,
                AllocationEndDate = null,
                AllocationStartDate = allocationStartDate,
                CreatedAt = allocationStartDate,
                CaseStatus = "Open",
                CaseClosureDate = null,
                LastModifiedAt = null,
                CreatedBy = worker.Email,
                LastModifiedBy = null,
                PersonId = person.Id,
                TeamId = team.Id,
                WorkerId = null
            };
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            //remove other allocations (created by the helpers) for simplicity
            DatabaseContext.Allocations.RemoveRange(DatabaseContext.Allocations);
            DatabaseContext.Allocations.Add(teamAllocation);
            DatabaseContext.SaveChanges();

            request.DeallocationScope = "team";

            var response = _classUnderTestWithProcessDataGateway.UpdateAllocation(request);

            var filter = Builders<BsonDocument>.Filter.Eq("mosaic_id", person.Id.ToString());

            var query = MongoDbTestContext.getCollection().AsQueryable().Where(db => filter.Inject());

            var result = query.ToList();

            result.Count.Should().Be(1);
            result.FirstOrDefault()["form_name"].Should().Be("Team deallocated");
            result.FirstOrDefault()["_id"].Should().Be(ObjectId.Parse(response.CaseNoteId));
        }

        [Test]
        public void CreatingAPersonShouldCreateCorrectRecordsAndAuditRecordsInTheDatabase()
        {
            //TODO: clean up before each test
            DatabaseContext.Audits.RemoveRange(DatabaseContext.Audits);
            DatabaseContext.Persons.RemoveRange(DatabaseContext.Persons);
            DatabaseContext.PersonOtherNames.RemoveRange(DatabaseContext.PersonOtherNames);
            DatabaseContext.Addresses.RemoveRange(DatabaseContext.Addresses);
            DatabaseContext.PhoneNumbers.RemoveRange(DatabaseContext.PhoneNumbers);
            DatabaseContext.PersonalRelationships.RemoveRange(DatabaseContext.PersonalRelationships);

            const string title = "Mr";
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();

            var otherNameFirstOne = _faker.Name.FirstName();
            var otherNameLastOne = _faker.Name.LastName();

            var otherNameFirstTwo = _faker.Name.FirstName();
            var otherNameLastTwo = _faker.Name.LastName();

            const string gender = "M";
            const string restricted = "Y";

            var otherNameOne = new OtherName() { FirstName = otherNameFirstOne, LastName = otherNameLastOne };
            var otherNameTwo = new OtherName() { FirstName = otherNameFirstTwo, LastName = otherNameLastTwo };

            var otherNames = new List<OtherName>() { otherNameOne, otherNameTwo };

            var dateOfBirth = DateTime.Now.AddYears(-70);
            var dateOfDeath = DateTime.Now.AddDays(-10);

            const string ethnicity = "A.A1"; //TODO test for valid values
            const string firstLanguage = "English";
            var religion = _faker.Random.Word();
            var sexualOrientation = _faker.Random.Word();
            long nhsNumber = _faker.Random.Number();

            const string addressLine = "1 Test Street";
            const long uprn = 1234567;
            const string postCode = "E18";
            const string isDisplayAddress = "Y";
            const string dataIsFromDmPersonsBackup = "N";

            var address = new AddressDomain() { Address = addressLine, Postcode = postCode, Uprn = uprn };

            const string phoneNumberOne = "07755555555";
            const string phoneNumberTypeOne = "Mobile";

            const string phoneNumberTwo = "07977777777";
            const string phoneNumberTypeTwo = "Fax";

            var phoneNumbers = new List<PhoneNumber>()
            {
                new PhoneNumber() {Number = phoneNumberOne, Type = phoneNumberTypeOne},
                new PhoneNumber() {Number = phoneNumberTwo, Type = phoneNumberTypeTwo}
            };

            var emailAddress = _faker.Internet.Email();
            var preferredMethodOfContact = _faker.Random.Word();
            const string contextFlag = "A"; //TODO test for valid values
            var createdBy = _faker.Internet.Email();

            AddNewResidentRequest request = new AddNewResidentRequest()
            {
                Title = title,
                FirstName = firstName,
                LastName = lastName,
                OtherNames = otherNames,
                Gender = gender,
                DateOfBirth = dateOfBirth,
                DateOfDeath = dateOfDeath,
                Ethnicity = ethnicity,
                FirstLanguage = firstLanguage,
                Religion = religion,
                SexualOrientation = sexualOrientation,
                NhsNumber = nhsNumber,
                Address = address,
                PhoneNumbers = phoneNumbers,
                EmailAddress = emailAddress,
                PreferredMethodOfContact = preferredMethodOfContact,
                ContextFlag = contextFlag,
                CreatedBy = createdBy,
                Restricted = restricted
            };

            var response = _classUnderTest.AddNewResident(request);

            Assert.IsNotNull(response.Id);

            Assert.IsNotNull(response.OtherNameIds);
            Assert.AreEqual(2, response.OtherNameIds.Count);

            Assert.IsNotNull(response.AddressId);

            Assert.IsNotNull(response.PhoneNumberIds);
            Assert.AreEqual(2, response.PhoneNumberIds.Count);

            //check that person was created with correct values
            var person = DatabaseContext.Persons.FirstOrDefault(x => x.Id == response.Id);

            Assert.IsNotNull(person);

            Assert.AreEqual(title, person.Title);
            Assert.AreEqual(firstName, person.FirstName);
            Assert.AreEqual(lastName, person.LastName);
            Assert.AreEqual(gender, person.Gender);
            Assert.AreEqual(dateOfBirth, person.DateOfBirth);
            Assert.AreEqual(dateOfDeath, person.DateOfDeath);
            Assert.AreEqual(ethnicity, person.Ethnicity);
            Assert.AreEqual(firstLanguage, person.FirstLanguage);
            Assert.AreEqual(religion, person.Religion);
            Assert.AreEqual(sexualOrientation, person.SexualOrientation);
            Assert.AreEqual(nhsNumber, person.NhsNumber);
            Assert.AreEqual(emailAddress, person.EmailAddress);
            Assert.AreEqual(preferredMethodOfContact, person.PreferredMethodOfContact);
            Assert.AreEqual(contextFlag, person.AgeContext);
            Assert.AreEqual(restricted, person.Restricted);

            //check that other names were created with correct values
            Assert.AreEqual(2, person.OtherNames.Count);
            Assert.IsTrue(person.OtherNames.Any(x => x.FirstName == otherNameFirstOne));
            Assert.IsTrue(person.OtherNames.Any(x => x.LastName == otherNameLastOne));
            Assert.IsTrue(person.OtherNames.Any(x => x.FirstName == otherNameFirstTwo));
            Assert.IsTrue(person.OtherNames.Any(x => x.LastName == otherNameLastTwo));
            Assert.IsTrue(person.OtherNames.All(x => x.PersonId == person.Id));

            //check that address was created with correct values
            Assert.IsNotNull(person.Addresses);
            Assert.AreEqual(addressLine, person.Addresses.First().AddressLines);
            Assert.AreEqual(postCode, person.Addresses.First().PostCode);
            Assert.AreEqual(uprn, person.Addresses.First().Uprn);
            Assert.AreEqual(isDisplayAddress, person.Addresses.First().IsDisplayAddress);
            Assert.AreEqual(dataIsFromDmPersonsBackup, person.Addresses.First().DataIsFromDmPersonsBackup);

            //check that phone numbers were created with correct values
            Assert.AreEqual(2, person.PhoneNumbers.Count);
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Number == phoneNumberOne));
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Type == phoneNumberTypeOne));
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Number == phoneNumberTwo));
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Type == phoneNumberTypeTwo));
            Assert.IsTrue(person.PhoneNumbers.All(x => x.PersonId == person.Id));

            //audit details
            Assert.AreEqual(createdBy, person.CreatedBy);
            Assert.IsNotNull(person.CreatedAt);

            Assert.IsTrue(person.Addresses.All(x => x.CreatedBy == createdBy));
            Assert.IsTrue(person.Addresses.All(x => x.CreatedBy != null));

            Assert.IsTrue(person.OtherNames.All(x => x.CreatedBy == createdBy));
            Assert.IsTrue(person.OtherNames.All(x => x.CreatedBy != null));

            Assert.IsTrue(person.PhoneNumbers.All(x => x.CreatedBy == createdBy));
            Assert.IsTrue(person.PhoneNumbers.All(x => x.CreatedBy != null));

            //TODO: create separate test setup for audit
        }

        [Test]
        public void CreatePersonRequestWithoutRestrictedValueSetShouldCreateRecordWithRestrictedValueSetToN()
        {
            var request = new AddNewResidentRequest()
            {
                FirstName = _faker.Person.FirstName,
                LastName = _faker.Person.LastName,
                ContextFlag = "A",
                CreatedBy = _faker.Internet.Email()
            };
            var response = _classUnderTest.AddNewResident(request);

            var person = DatabaseContext.Persons.First(x => x.Id == response.Id);

            person.Restricted.Should().Be("N");
            person.FirstName.Should().Be(request.FirstName);
            person.LastName.Should().Be(request.LastName);
            person.AgeContext.Should().Be(request.ContextFlag);
            person.CreatedBy.Should().Be(request.CreatedBy);
        }

        [Test]
        public void PostWarningNoteShouldInsertIntoTheDatabase()
        {
            var stubbedPerson = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(stubbedPerson);
            DatabaseContext.SaveChanges();

            var request = _fixture.Build<PostWarningNoteRequest>()
                .With(x => x.PersonId, stubbedPerson.Id)
                .Create();

            var response = _classUnderTest.PostWarningNote(request);

            var query = DatabaseContext.WarningNotes;

            var insertedRecord = query.FirstOrDefault(x => x.Id == response.WarningNoteId);

            insertedRecord?.PersonId.Should().Be(request.PersonId);
            insertedRecord?.Status.Should().Be("open");

            insertedRecord?.StartDate.Should().Be(request.StartDate);
            insertedRecord?.EndDate.Should().Be(request.EndDate);

            insertedRecord?.ReviewDate.Should().Be(request.ReviewDate);
            insertedRecord?.NextReviewDate.Should().Be(request.NextReviewDate);

            insertedRecord?.DisclosedWithIndividual.Should().Be(request.DisclosedWithIndividual);
            insertedRecord?.DisclosedDetails.Should().Be(request.DisclosedDetails);
            insertedRecord?.DisclosedDate.Should().Be(request.DisclosedDate);
            insertedRecord?.DisclosedHow.Should().Be(request.DisclosedHow);

            insertedRecord?.Notes.Should().Be(request.Notes);
            insertedRecord?.NoteType.Should().Be(request.NoteType);
            insertedRecord?.WarningNarrative.Should().Be(request.WarningNarrative);

            insertedRecord?.ManagerName.Should().Be(request.ManagerName);
            insertedRecord?.DiscussedWithManagerDate.Should().Be(request.DiscussedWithManagerDate);

            insertedRecord?.CreatedBy.Should().Be(request.CreatedBy);

            //audit properties
            insertedRecord?.CreatedAt.Should().NotBeNull();
            insertedRecord?.CreatedAt.Should().NotBe(DateTime.MinValue);

            insertedRecord?.LastModifiedAt.Should().BeNull();
            insertedRecord?.LastModifiedBy.Should().BeNull();
        }

        [Test]
        public void PostWarningNoteShouldThrowAnErrorIfThePersonIdIsNotInThePersonTable()
        {
            var request = _fixture.Create<PostWarningNoteRequest>();

            Action act = () => _classUnderTest.PostWarningNote(request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"Person with given id ({request.PersonId}) not found");
        }

        [Test]
        public void PostWarningNoteShouldCallInsertCaseNoteMethod()
        {
            Person stubbedPerson = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(stubbedPerson);
            DatabaseContext.SaveChanges();

            var request = _fixture.Build<PostWarningNoteRequest>()
                .With(x => x.PersonId, stubbedPerson.Id)
                .Create();

            _mockProcessDataGateway.Setup(
                    x => x.InsertCaseNoteDocument(
                        It.IsAny<CaseNotesDocument>()))
                .ReturnsAsync("CaseNoteId")
                .Verifiable();

            var response = _classUnderTest.PostWarningNote(request);

            _mockProcessDataGateway.Verify();
            response.CaseNoteId.Should().Be("CaseNoteId");
        }

        [Test]
        public void GetWarningNotesReturnsTheExpectedWarningNote()
        {
            var testPersonId = _faker.Random.Int();

            var warningNote = TestHelpers.CreateWarningNote(testPersonId);

            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.GetWarningNotes(testPersonId).ToList();
            var retrievedWarningNote = response.FirstOrDefault();

            response.Should().ContainSingle();

            retrievedWarningNote?.Id.Should().Be(warningNote.Id);
            retrievedWarningNote?.Status.Should().Be("open");

            retrievedWarningNote?.PersonId.Should().Be(warningNote.PersonId);

            retrievedWarningNote?.WarningNarrative.Should().Be(warningNote.WarningNarrative);

            retrievedWarningNote?.StartDate.Should().Be(warningNote.StartDate);
            retrievedWarningNote?.EndDate.Should().Be(warningNote.EndDate);

            retrievedWarningNote?.ReviewDate.Should().Be(warningNote.ReviewDate);
            retrievedWarningNote?.NextReviewDate.Should().Be(warningNote.NextReviewDate);

            retrievedWarningNote?.DisclosedWithIndividual.Should().Be(warningNote.DisclosedWithIndividual);
            retrievedWarningNote?.DisclosedDetails.Should().Be(warningNote.DisclosedDetails);
            retrievedWarningNote?.DisclosedDate.Should().Be(warningNote.DisclosedDate);
            retrievedWarningNote?.DisclosedHow.Should().Be(warningNote.DisclosedHow);

            retrievedWarningNote?.Notes.Should().Be(warningNote.Notes);
            retrievedWarningNote?.NoteType.Should().Be(warningNote.NoteType);

            retrievedWarningNote?.ManagerName.Should().Be(warningNote.ManagerName);
            retrievedWarningNote?.DiscussedWithManagerDate.Should().Be(warningNote.DiscussedWithManagerDate);

            retrievedWarningNote?.CreatedBy.Should().Be(warningNote.CreatedBy);
        }

        [Test]
        public void GetWarningNotesReturnsAListOfWarningNotesForASpecificPerson()
        {
            var testPersonId = _faker.Random.Int();

            var firstWarningNote = TestHelpers.CreateWarningNote(testPersonId);
            var secondWarningNote = TestHelpers.CreateWarningNote(testPersonId);

            var differentWarningNote = TestHelpers.CreateWarningNote();

            DatabaseContext.WarningNotes.Add(firstWarningNote);
            DatabaseContext.WarningNotes.Add(secondWarningNote);
            DatabaseContext.WarningNotes.Add(differentWarningNote);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.GetWarningNotes(testPersonId).ToList();

            response.Count.Should().Be(2);
            response.Should().ContainEquivalentOf(firstWarningNote);
            response.Should().ContainEquivalentOf(secondWarningNote);
            response.Should().NotContain(differentWarningNote);
        }

        [Test]
        public void GetWarningNotesReturnsAnEmptyListIfNoWarningNotesExist()
        {
            var response = _classUnderTest.GetWarningNotes(123);

            response.Should().BeEmpty();
        }

        [Test]
        public void PatchWarningNoteUpdatesAnExistingRecordInTheDatabase()
        {
            var fakeTime = new DateTime(2000, 1, 1);
            _mockSystemTime.Setup(time => time.Now).Returns(fakeTime);

            var (request, person, worker, warningNote) =
                TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "closed");

            //clone the stub to compare the values later
            WarningNote stubbedWarningNote = (WarningNote) warningNote.Clone();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.EndDate.Should().Be(fakeTime);
            updatedRecord.LastReviewDate.Should().Be(request.ReviewDate);
            updatedRecord.NextReviewDate.Should().BeNull();
            updatedRecord.Status.Should().Be("closed");
            updatedRecord.Status.Should().NotBe(stubbedWarningNote.Status);
            updatedRecord.LastModifiedBy.Should().Be(request.ReviewedBy);
            updatedRecord.LastModifiedBy.Should().NotBe(stubbedWarningNote.LastModifiedBy);

            updatedRecord.Id.Should().Be(stubbedWarningNote.Id);
            updatedRecord.PersonId.Should().Be(stubbedWarningNote.PersonId);
            updatedRecord.StartDate.Should().Be(stubbedWarningNote.StartDate);
            updatedRecord.DisclosedWithIndividual.Should().Be(stubbedWarningNote.DisclosedWithIndividual);
            updatedRecord.DisclosedDetails.Should().Be(stubbedWarningNote.DisclosedDetails);
            updatedRecord.Notes.Should().Be(stubbedWarningNote.Notes);
            updatedRecord.NoteType.Should().Be(stubbedWarningNote.NoteType);
            updatedRecord.DisclosedDate.Should().Be(stubbedWarningNote.DisclosedDate);
            updatedRecord.DisclosedHow.Should().Be(stubbedWarningNote.DisclosedHow);
            updatedRecord.WarningNarrative.Should().Be(stubbedWarningNote.WarningNarrative);
            updatedRecord.ManagerName.Should().Be(stubbedWarningNote.ManagerName);
            updatedRecord.DiscussedWithManagerDate.Should().Be(stubbedWarningNote.DiscussedWithManagerDate);
            updatedRecord.CreatedBy.Should().Be(stubbedWarningNote.CreatedBy);
        }

        [Test]
        public void PatchWarningNoteDoesNotChangeTheStatusEndDateOrChangeNextReviewDateToNullIfRequestPropertyIsOpen()
        {
            var (request, person, worker, warningNote) =
                TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "open");

            //clone the stub to compare the values later
            var stubbedWarningNote = (WarningNote) warningNote.Clone();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.Status.Should().Be(stubbedWarningNote.Status);
            updatedRecord.EndDate.Should().Be(stubbedWarningNote.EndDate);
            updatedRecord.NextReviewDate.Should().Be(request.NextReviewDate);
        }

        [Test]
        public void PatchWarningNoteClosesTheWarningNoteIfRequestStatusIsClosed()
        {
            var fakeTime = new DateTime(2000, 1, 1);

            _mockSystemTime.Setup(time => time.Now).Returns(fakeTime);

            var (request, person, worker, warningNote) =
                TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "closed");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.Status.Should().Be("closed");
            updatedRecord.EndDate.Should().Be(fakeTime);
            updatedRecord.NextReviewDate.Should().BeNull();
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenTheWarningNoteIsNotInTheWarningNoteTable()
        {
            PatchWarningNoteRequest request = new PatchWarningNoteRequest();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                .WithMessage($"Warning Note with given id ({request.WarningNoteId}) not found");
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenTheWarningNoteIsAlreadyClosed()
        {
            var (request, person, worker, warningNote) =
                TestHelpers.CreatePatchWarningNoteRequest(startingStatus: "closed", requestStatus: "closed");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                .WithMessage($"Warning Note with given id ({request.WarningNoteId}) has already been closed");
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenNoPersonPresentInDatabase()
        {
            WarningNote stubbedWarningNote = TestHelpers.CreateWarningNote();
            DatabaseContext.WarningNotes.Add(stubbedWarningNote);
            DatabaseContext.SaveChanges();

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                .Create();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                .WithMessage($"Person not found");
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenReviewerIsNotPresentInWorkerTable()
        {
            Person stubbedPerson = TestHelpers.CreatePerson();
            WarningNote stubbedWarningNote = TestHelpers.CreateWarningNote(personId: stubbedPerson.Id);

            DatabaseContext.Persons.Add(stubbedPerson);
            DatabaseContext.WarningNotes.Add(stubbedWarningNote);
            DatabaseContext.SaveChanges();

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                .Create();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                .WithMessage($"Worker ({request.ReviewedBy}) not found");
        }

        [Test]
        public void PatchWarningNoteAddsAWarningNoteReviewToTheDatabase()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();


            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNoteReview;
            var insertedRecord = query.FirstOrDefault(
                x => x.WarningNoteId == request.WarningNoteId);

            insertedRecord?.WarningNoteId.Should().Be(request.WarningNoteId);
            insertedRecord?.ReviewDate.Should().Be(request.ReviewDate);
            insertedRecord?.DisclosedWithIndividual.Should().Be(request.DisclosedWithIndividual);
            insertedRecord?.ReviewNotes.Should().Be(request.ReviewNotes);
            insertedRecord?.ManagerName.Should().Be(request.ManagerName);
            insertedRecord?.DiscussedWithManagerDate.Should().Be(request.DiscussedWithManagerDate);
            insertedRecord?.CreatedBy.Should().Be(request.ReviewedBy);
            insertedRecord?.LastModifiedBy.Should().Be(request.ReviewedBy);
        }

        [Test]
        public void PatchWarningNotesCreatesReviewedCaseHistoryNoteByCallingProcessDataGatewayWhenRequestStatusNotClosed()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            _classUnderTestWithProcessDataGateway.PatchWarningNote(request);

            var filter = Builders<BsonDocument>.Filter.Eq("mosaic_id", person.Id.ToString());

            var result = MongoDbTestContext.getCollection().Find(filter).First();

            string note = $"Warning note against this person reviewed";

            result["first_name"].Should().Be(person.FirstName);
            result["last_name"].Should().Be(person.LastName);
            result["mosaic_id"].Should().Be(person.Id.ToString());
            result["note"].ToString().Should().Contain(note);
            result["form_name_overall"].Should().Be("API_WarningNote");
            result["form_name"].Should().Be("Warning Note Reviewed");
            result["worker_email"].Should().Be(request.ReviewedBy);
        }

        [Test]
        public void PatchWarningNotesCreatesClosedCaseHistoryNoteByCallingProcessDataGatewayWhenRequestStatusIsClosed()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "closed");
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            _classUnderTestWithProcessDataGateway.PatchWarningNote(request);

            var filter = Builders<BsonDocument>.Filter.Eq("mosaic_id", person.Id.ToString());

            var result = MongoDbTestContext.getCollection().Find(filter).First();

            string note = $"Warning note against this person ended";

            result["first_name"].Should().Be(person.FirstName);
            result["last_name"].Should().Be(person.LastName);
            result["mosaic_id"].Should().Be(person.Id.ToString());
            result["note"].ToString().Should().Contain(note);
            result["form_name_overall"].Should().Be("API_WarningNote");
            result["form_name"].Should().Be("Warning Note Ended");
            result["worker_email"].Should().Be(request.ReviewedBy);
        }

        [Test]
        public void GetWarningNoteByIdReturnTheSpecifiedWarningNote()
        {
            var newWarningNote = TestHelpers.CreateWarningNote();
            DatabaseContext.WarningNotes.Add(newWarningNote);
            DatabaseContext.SaveChanges();
            var expectedResponse = newWarningNote.ToDomain();

            var response = _classUnderTest.GetWarningNoteById(newWarningNote.Id);

            response.Should().BeEquivalentTo(expectedResponse);
            response.WarningNoteReviews.Should().BeEmpty();
        }

        [Test]
        public void GetWarningNoteByIdReturnsNullWhenThereAreNoMatchingRecords()
        {
            var response = _classUnderTest.GetWarningNoteById(123);

            response.Should().BeNull();
        }

        [Test]
        public void GetWarningNoteByIdReturnsAReviewWithTheWarningNoteIfAny()
        {
            var warningNote = TestHelpers.CreateWarningNote();
            var review = TestHelpers.CreateWarningNoteReview(warningNote.Id);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.WarningNoteReview.Add(review);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.GetWarningNoteById(warningNote.Id);

            response.WarningNoteReviews.Count.Should().Be(1);
            response.WarningNoteReviews.FirstOrDefault().Should().BeEquivalentTo(review);
        }

        [Test]
        public void GetWarningNoteByIdReturnsReturnsMultipleReviewsWithTheWarningNote()
        {
            var warningNote = TestHelpers.CreateWarningNote();
            var firstReview = TestHelpers.CreateWarningNoteReview(warningNote.Id);
            var secondReview = TestHelpers.CreateWarningNoteReview(warningNote.Id, disclosedWithIndividual: true);
            var thirdReview = TestHelpers.CreateWarningNoteReview(warningNote.Id, disclosedWithIndividual: false);

            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.WarningNoteReview.Add(firstReview);
            DatabaseContext.WarningNoteReview.Add(secondReview);
            DatabaseContext.WarningNoteReview.Add(thirdReview);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.GetWarningNoteById(warningNote.Id);

            response.WarningNoteReviews.Count.Should().Be(3);
            response.WarningNoteReviews.Should().ContainEquivalentOf(firstReview);
            response.WarningNoteReviews.Should().ContainEquivalentOf(secondReview);
            response.WarningNoteReviews.Should().ContainEquivalentOf(thirdReview);
        }

        [Test]
        public void GetWarningNoteByIdShouldNotReturnReviewsMadeForADifferentWarningNote()
        {
            var warningNote = TestHelpers.CreateWarningNote();
            var differentWarningNote = TestHelpers.CreateWarningNote();

            var review = TestHelpers.CreateWarningNoteReview(warningNote.Id);
            var differentReview = TestHelpers.CreateWarningNoteReview(differentWarningNote.Id);

            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.WarningNotes.Add(differentWarningNote);
            DatabaseContext.WarningNoteReview.Add(review);
            DatabaseContext.WarningNoteReview.Add(differentReview);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.GetWarningNoteById(warningNote.Id);

            response.WarningNoteReviews.Count.Should().Be(1);
            response.WarningNoteReviews.Should().ContainEquivalentOf(review);
            response.WarningNoteReviews.Should().NotContain(differentReview);
        }

        private Worker SaveWorkerToDatabase(Worker worker)
        {
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();
            return worker;
        }

        private Team SaveTeamToDatabase(Team team)
        {
            DatabaseContext.Teams.Add(team);
            DatabaseContext.SaveChanges();
            return team;
        }

        [Test]
        public void GetPersonDetailsByIdReturnsPerson()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            //add all necessary related entities using person id
            SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id));
            SavePhoneNumberToDataBase(DatabaseGatewayHelper.CreatePhoneNumberEntity(person.Id));
            SavePersonOtherNameToDatabase(DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id));

            var response = _classUnderTest.GetPersonDetailsById(person.Id);

            response.Should().BeEquivalentTo(person);
        }

        [Test]
        public void GetPersonDetailsByIdDoesNotReturnPersonMarkedForDeletion()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(markedForDeletion: true));

            var response = _classUnderTest.GetPersonDetailsById(person.Id);

            response.Should().BeNull();
        }

        [Test]
        public void UpdatePersonThrowsUpdatePersonExceptionWhenPersonNotFound()
        {
            Action act = () => _classUnderTest.UpdatePerson(new UpdatePersonRequest() { Id = _faker.Random.Long() });

            act.Should().Throw<UpdatePersonException>().WithMessage("Person not found");
        }

        [Test]
        public void UpdatePersonSetsCorrectValuesForPersonEntity()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            person.AgeContext.Should().Be(request.ContextFlag);
            person.DateOfBirth.Should().Be(request.DateOfBirth);
            person.DateOfDeath.Should().Be(request.DateOfDeath);
            person.EmailAddress.Should().Be(request.EmailAddress);
            person.Ethnicity.Should().Be(request.Ethnicity);
            person.FirstLanguage.Should().Be(request.FirstLanguage);
            person.FirstName.Should().Be(request.FirstName);
            person.FullName.Should().Be($"{request.FirstName} {request.LastName}");
            person.Gender.Should().Be(request.Gender);
            person.LastModifiedBy.Should().Be(request.CreatedBy);
            person.LastName.Should().Be(request.LastName);
            person.NhsNumber.Should().Be(request.NhsNumber);
            person.PreferredMethodOfContact.Should().Be(request.PreferredMethodOfContact);
            person.Religion.Should().Be(request.Religion);
            person.Restricted.Should().Be(request.Restricted);
            person.SexualOrientation.Should().Be(request.SexualOrientation);
            person.Title.Should().Be(request.Title);
            person.ReviewDate.Should().Be(request.ReviewDate);
        }

        [Test]
        public void UpdatePersonsAddressUpdatesTheMostRecentAddressToDisplay()
        {
            var first = DateTime.Today;
            var second = DateTime.Today.AddDays(-1);
            var last = DateTime.Today.AddDays(-2);

            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            person.Addresses = null;

            var firstAddress = DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, startDate: first, isDisplayAddress: "Y");
            var secondAddress = DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, startDate: second, isDisplayAddress: "Y");
            var lastAddress = DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, startDate: last, isDisplayAddress: "Y");
            DatabaseContext.AddRange(new List<dbAddress>() { firstAddress, secondAddress, lastAddress });

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Addresses.Add(firstAddress);
            DatabaseContext.Addresses.Add(secondAddress);
            DatabaseContext.Addresses.Add(lastAddress);
            DatabaseContext.SaveChanges();

            person.Addresses.Count.Should().Equals(3);
            firstAddress.IsDisplayAddress.Should().Equals("Y");

            var request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);
            var newAddress = DatabaseContext.Addresses.FirstOrDefault(x => x.PostCode == request.Address.Postcode && x.PersonId == person.Id);

            person.Addresses.Count.Should().Equals(4);
            firstAddress.EndDate.Should().NotBeNull();
            firstAddress.IsDisplayAddress.Should().Equals("N");

            newAddress.IsDisplayAddress.Should().Equals("Y");
            newAddress.StartDate.Should().BeWithin(TimeSpan.FromSeconds(1)).Before(DateTime.Now);
        }

        [Test]
        public void PatchPersonSetsCorrectValuesForPersonEntity()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Foo"));
            PatchPersonRequest request = GetValidPatchPersonRequest(person.Id);

            _classUnderTest.PatchPerson(request);

            person.Title.Should().Be(request.Title);
            person.FirstName.Should().Be(request.FirstName);
            person.LastName.Should().Be(request.LastName);
            person.Gender.Should().Be(request.Gender);
            person.DateOfBirth.Should().Be(request.DateOfBirth);
            person.DateOfDeath.Should().Be(request.DateOfDeath);
            person.Ethnicity.Should().Be(request.Ethnicity);
            person.FirstLanguage.Should().Be(request.FirstLanguage);
            person.Religion.Should().Be(request.Religion);
            person.SexualOrientation.Should().Be(request.SexualOrientation);
            person.NhsNumber.Should().Be(request.NhsNumber);
            person.FluentInEnglish.Should().Be(request.FluentInEnglish);
            person.InterpreterNeeded.Should().Be(request.InterpreterNeeded);
            person.CommunicationDifficulties.Should().Be(request.CommunicationDifficulties);
            person.DifficultyMakingDecisions.Should().Be(request.DifficultyMakingDecisions);
            person.CommunicationDifficultiesDetails.Should().Be(request.CommunicationDifficultiesDetails);
            person.Employment.Should().Be(request.Employment);
            person.AllocatedTeam.Should().Be(request.AllocatedTeam);
            person.PreferredLanguage.Should().Be(request.PreferredLanguage);
            person.Nationality.Should().Be(request.Nationality);
            person.Pronoun.Should().Be(request.Pronoun);
            person.GenderAssignedAtBirth.Should().Be(request.GenderAssignedAtBirth);
            person.MaritalStatus.Should().Be(request.MaritalStatus);
            person.ImmigrationStatus.Should().Be(request.ImmigrationStatus);
            person.PrimarySupportReason.Should().Be(request.PrimarySupportReason);
            person.CareProvider.Should().Be(request.CareProvider);
            person.LivingSituation.Should().Be(request.LivingSituation);
            person.AccomodationType.Should().Be(request.AccomodationType);
            person.AccessToHome.Should().Be(request.AccessToHome);
            person.HousingOfficer.Should().Be(request.HousingOfficer);
            person.HousingStaffInContact.Should().Be(request.HousingStaffInContact);
            person.CautionaryAlert.Should().Be(request.CautionaryAlert);
            person.PossessionEvictionOrder.Should().Be(request.PossessionEvictionOrder);
            person.FirstName.Should().Be(request.FirstName);
            person.RentRecord.Should().Be(request.RentRecord);
            person.HousingBenefit.Should().Be(request.HousingBenefit);
            person.CouncilTenureType.Should().Be(request.CouncilTenureType);
            person.MentalHealthSectionStatus.Should().Be(request.MentalHealthSectionStatus);
            person.DeafRegister.Should().Be(request.DeafRegister);
            person.BlindRegister.Should().Be(request.BlindRegister);
            person.BlueBadge.Should().Be(request.BlueBadge);
            person.OpenCase.Should().Be(request.OpenCase);
            person.LastUpdated.Should().BeEquivalentTo(request.LastUpdated);
            person.OtherNames.Should().BeEquivalentTo(request.OtherNames);
            person.PhoneNumbers.Should().BeEquivalentTo(request.PhoneNumbers);
            person.LastUpdated.Should().BeEquivalentTo(request.LastUpdated);
            person.KeyContacts.Should().BeEquivalentTo(request.KeyContacts);
            person.GpDetails.Should().BeEquivalentTo(request.GpDetails);
            person.TechUse[0].TechType.Should().BeEquivalentTo(request.TechUse[0]);
            person.Disability[0].DisabilityType.Should().BeEquivalentTo(request.Disabilities[0]);
            person.Emails.Should().BeEquivalentTo(request.Emails);
            person.ReviewDate.Should().Be(request.ReviewDate);
        }

        [Test]
        public void PatchPersonRemovesListTypesForPersonEntityWhenNullValueIsRequested()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Foo"));
            PatchPersonRequest request = GetValidPatchPersonRequest(person.Id);

            request.LastUpdated = null;
            request.KeyContacts = null;
            request.GpDetails = null;
            request.TechUse = null;
            request.Disabilities = null;
            request.Emails = null;
            request.PhoneNumbers = null;

            _classUnderTest.PatchPerson(request);

            person.LastUpdated.Should().BeEmpty();
            person.PhoneNumbers.Should().BeEmpty();
            person.LastUpdated.Should().BeEmpty();
            person.KeyContacts.Should().BeEmpty();
            person.TechUse.Should().BeEmpty();
            person.Disability.Should().BeEmpty();
            person.Emails.Should().BeEmpty();
        }

        [Test]
        public void PatchPersonDoesNotUpdateCreatedByProperty()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            var request = GetValidPatchPersonRequest(person.Id);

            var expectedCreatedBy = person.CreatedBy;
            request.CreatedBy = "first.last@domain.com";

            _classUnderTest.PatchPerson(request);

            person.CreatedBy.Should().Be(expectedCreatedBy);
        }

        [Test]
        public void UpdatePersonSetsNewDisplayAddressWhenOneDoesntExist()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            person.Addresses.Count.Should().Be(1);
            person.Addresses.First().IsDisplayAddress.Should().Be("Y");
            person.Addresses.First(x => x.IsDisplayAddress == "Y").AddressLines.Should().Be(request.Address.Address);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").PostCode.Should().Be(request.Address.Postcode);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").Uprn.Should().Be(request.Address.Uprn);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").CreatedBy.Should().Be(request.CreatedBy);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").CreatedAt.Should().NotBeNull();
            person.Addresses.First().StartDate.Should().NotBeNull();
        }

        [Test]
        public void UpdatePersonUpdatesTheCurrentDisplayAddressToBeHistoricalAddress()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            var displayAddress =
                SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, "Y"));
            var request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            person.Addresses.Count.Should().Be(2);

            displayAddress.IsDisplayAddress.Should().Be("N");
            displayAddress.LastModifiedBy.Should().Be(request.CreatedBy);
            displayAddress.EndDate.Should().NotBeNull();
        }

        [Test]
        public void UpdatePersonDoesntUpdateCurrentDisplayAddressIfAddressHasNotChanged()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            dbAddress displayAddress =
                SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, "Y"));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            request.Address.Address = displayAddress.AddressLines;
            request.Address.Postcode = displayAddress.PostCode;
            request.Address.Uprn = displayAddress.Uprn;

            _classUnderTest.UpdatePerson(request);

            person.Addresses.First().AddressLines.Should().Be(request.Address.Address);
            person.Addresses.First().PostCode.Should().Be(request.Address.Postcode);
            person.Addresses.First().Uprn.Should().Be(request.Address.Uprn);

            person.Addresses.First().IsDisplayAddress.Should().Be("Y");
            person.Addresses.First().EndDate.Should().BeNull();
        }

        [Test]
        public void UpdatePersonSetsTheCurrentDisplayAddressNotToBeDisplayAddressWhenAddressIsNotProvided()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, "Y"));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            request.Address = null;

            _classUnderTest.UpdatePerson(request);

            person.Addresses.First().IsDisplayAddress.Should().Be("N");
            person.Addresses.First().LastModifiedBy.Should().Be(request.CreatedBy);
            person.Addresses.First().EndDate.Should().NotBeNull();
        }

        [Test]
        public void UpdatePersonReplacesPhoneNumbers()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            var request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            var number1 = request.PhoneNumbers.First();
            var number2 = request.PhoneNumbers.Last();

            person.PhoneNumbers.First(x => x.Number == number1.Number).Type.Should().Be(number1.Type);
            person.PhoneNumbers.First(x => x.Number == number2.Number).Type.Should().Be(number2.Type);

            person.PhoneNumbers.First().CreatedBy.Should().Be(request.CreatedBy);
            person.PhoneNumbers.First().CreatedAt.Should().NotBeNull();

            person.PhoneNumbers.Last().CreatedBy.Should().Be(request.CreatedBy);
            person.PhoneNumbers.Last().CreatedAt.Should().NotBeNull();

            person.PhoneNumbers.Count.Should().Be(request.PhoneNumbers.Count);
        }

        [Test]
        public void UpdatePersonRemovesPhoneNumbersIfNewOnesAreNotProvided()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            var request = GetValidUpdatePersonRequest(person.Id);
            request.PhoneNumbers = null;

            _classUnderTest.UpdatePerson(request);

            person.PhoneNumbers.Count.Should().Be(0);
        }

        [Test]
        public void UpdatePersonReplacesOtherNames()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            SavePersonOtherNameToDatabase(DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id));
            var request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            var name1 = request.OtherNames.First();
            var name2 = request.OtherNames.Last();

            person.OtherNames.First(x => x.FirstName == name1.FirstName).LastName.Should().Be(name1.LastName);
            person.OtherNames.First(x => x.FirstName == name2.FirstName).LastName.Should().Be(name2.LastName);

            person.OtherNames.First().CreatedBy.Should().Be(request.CreatedBy);
            person.OtherNames.First().CreatedAt.Should().NotBeNull();

            person.OtherNames.Last().CreatedBy.Should().Be(request.CreatedBy);
            person.OtherNames.Last().CreatedAt.Should().NotBeNull();

            person.OtherNames.Count.Should().Be(request.PhoneNumbers.Count);
        }

        [Test]
        public void UpdatePersonRemovesOtherNamesIfNewOnesAreNotProvided()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            SavePersonOtherNameToDatabase(DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id));

            var request = GetValidUpdatePersonRequest(person.Id);
            request.OtherNames = null;

            _classUnderTest.UpdatePerson(request);

            person.OtherNames.Count.Should().Be(0);
        }

        [Test]
        public void UpdatePersonUpdatesFullNameWhenFirstNameIsUpdated()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "First", lastName: "Last"));

            var request = GetValidPatchPersonRequest(person.Id);

            request.FirstName = "FirstUpdated";
            request.LastName = null;

            _classUnderTest.PatchPerson(request);

            var updatedPerson = DatabaseContext.Persons.FirstOrDefault(x => x.Id == person.Id);

            updatedPerson.FullName.Should().Be($"{request.FirstName} {person.LastName}");
        }

        [Test]
        public void UpdatePersonUpdatesFullNameWhenLastNameIsUpdated()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "First", lastName: "Last"));

            var request = GetValidPatchPersonRequest(person.Id);

            request.FirstName = null;
            request.LastName = "LastUpdated";

            _classUnderTest.PatchPerson(request);

            var updatedPerson = DatabaseContext.Persons.FirstOrDefault(x => x.Id == person.Id);

            updatedPerson.FullName.Should().Be($"{person.FirstName} {request.LastName}");
        }

        [Test]
        public void UpdatePersonCreatesCorrectCaseHistoryNoteByCallingProcessDataGateway()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            var request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTestWithProcessDataGateway.UpdatePerson(request);

            var filter = Builders<BsonDocument>.Filter.Eq("mosaic_id", person.Id.ToString());

            var result = MongoDbTestContext.getCollection().Find(filter).First();

            var note = $"Person details updated - by {request.CreatedBy}."; //TODO: work out timestamp handling

            result["note"].ToString().Should().Contain(note);
            result["created_by"].Should().Be(request.CreatedBy);
            result["first_name"].Should().Be(person.FirstName);
            result["last_name"].Should().Be(person.LastName);
            result["mosaic_id"].Should().Be(person.Id.ToString());
            result["worker_email"].Should().Be(request.CreatedBy);
            result["form_name_overall"].Should().Be("API_Update_Person");
            result["form_name"].Should().Be("Person updated");
            result["is_imported"].Should().Be(false);
        }

        [Test]
        public void GetPersonsByListOfIdsReturnsCorrectPersonRecordsBasedOnGivenIds()
        {
            var personOne = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            var personTwo = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            var listOfPersonRecords = new List<Person>() { personOne, personTwo };

            var result = _classUnderTest.GetPersonsByListOfIds(listOfPersonRecords.Select(x => x.Id).ToList());

            result.Should().BeEquivalentTo(listOfPersonRecords);
        }

        [Test]
        public void GetPersonsByListOfIdsDoesNotReturnPersonRecordsMarkedForDeletion()
        {
            var personOne = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            var personTwo = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(markedForDeletion: true));

            var listOfPersonRecords = new List<Person>() { personOne, personTwo };

            var result = _classUnderTest.GetPersonsByListOfIds(listOfPersonRecords.Select(x => x.Id).ToList());

            result.Count.Should().Be(1);
            result.First().Id.Should().Be(personOne.Id);
        }

        [Test]
        public void GetPersonsByListOfIdsReturnsEmptyListWhenNoMatchingPersonsFound()
        {
            var personIds = _fixture.Create<List<long>>();

            var result = _classUnderTest.GetPersonsByListOfIds(personIds);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetPersonByMosaicIdReturnsCorrectPersonRecord()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            var result = _classUnderTest.GetPersonByMosaicId(person.Id);

            result.Should().BeEquivalentTo(person);
        }

        [Test]
        public void GetPersonByMosaicIdReturnsNullWhenPersonNotFound()
        {
            var result = _classUnderTest.GetPersonByMosaicId(0L);

            result.Should().BeNull();
        }

        [Test]
        public void GetPersonIdsByEmergencyIdReturnsEmptyListWhenNoMatchingRecordsFound()
        {
            _classUnderTest.GetPersonIdsByEmergencyId("123").Count.Should().Be(0);
        }

        [Test]
        public void GetPersonIdsByEmergencyIdReturnsListOfMatchingPersonIds()
        {
            var person1 = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 33004455));
            var person2 = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 33007788));
            var person3 = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 33009900));

            DatabaseContext.PersonLookups.Add(
                new PersonIdLookup()
                {
                    MosaicId = person1.Id.ToString(),
                    NCId = "NC123"
                }
            );

            DatabaseContext.PersonLookups.Add(
               new PersonIdLookup()
               {
                   MosaicId = person2.Id.ToString(),
                   NCId = "TMP123"
               }
           );

            DatabaseContext.SaveChanges();

            var result = _classUnderTest.GetPersonIdsByEmergencyId("123");

            result.Count.Should().Be(2);
            result.Any(x => x == person1.Id).Should().BeTrue();
            result.Any(x => x == person2.Id).Should().BeTrue();
            result.Any(x => x == person3.Id).Should().BeFalse();
        }

        [Test]
        [TestCase("041")]
        [TestCase("0041")]
        [TestCase("00041")]
        [TestCase("000041")]
        public void GetPersonIdsByEmergencyIdReturnsListOfMatchingPersonIdsWhenEmergencyIdHasLeadingZerosInId(string id)
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 33004455));

            DatabaseContext.PersonLookups.Add(
                new PersonIdLookup()
                {
                    MosaicId = person.Id.ToString(),
                    NCId = id
                });

            DatabaseContext.SaveChanges();

            var result = _classUnderTest.GetPersonIdsByEmergencyId(id);

            result.Count.Should().Be(1);
        }

        [Test]
        public void GetPersonByMosaicIdReturnsNullWhenPersonIsMarkedForDeletion()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity(markedForDeletion: true));

            var result = _classUnderTest.GetPersonByMosaicId(person.Id);

            result.Should().BeNull();
        }

        private Person SavePersonToDatabase(Person person)
        {
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();
            return person;
        }

        private dbAddress SaveAddressToDatabase(dbAddress address)
        {
            DatabaseContext.Addresses.Add(address);
            DatabaseContext.SaveChanges();
            return address;
        }

        private void SavePhoneNumberToDataBase(PhoneNumberInfrastructure phoneNumber)
        {
            DatabaseContext.PhoneNumbers.Add(phoneNumber);
            DatabaseContext.SaveChanges();
        }

        private void SavePersonOtherNameToDatabase(PersonOtherName name)
        {
            DatabaseContext.PersonOtherNames.Add(name);
            DatabaseContext.SaveChanges();
        }

        private static UpdatePersonRequest GetValidUpdatePersonRequest(long personId)
        {
            AddressDomain address = new Faker<AddressDomain>()
                .RuleFor(a => a.Address, f => f.Address.StreetAddress())
                .RuleFor(a => a.Postcode, f => f.Address.ZipCode())
                .RuleFor(a => a.Uprn, f => f.Random.Int());

            var phoneNumbers = new List<PhoneNumber>()
            {
                new Faker<PhoneNumber>()
                    .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                    .RuleFor(p => p.Type, f => f.Random.Word()),
                new Faker<PhoneNumber>()
                    .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                    .RuleFor(p => p.Type, f => f.Random.Word())
            };

            var otherNames = new List<OtherName>()
            {
                new Faker<OtherName>()
                    .RuleFor(o => o.FirstName, f => f.Person.FirstName)
                    .RuleFor(o => o.LastName, f => f.Person.LastName),
                new Faker<OtherName>()
                    .RuleFor(o => o.FirstName, f => f.Person.FirstName)
                    .RuleFor(o => o.LastName, f => f.Person.LastName)
            };

            return new Faker<UpdatePersonRequest>()
                .RuleFor(p => p.Id, personId)
                .RuleFor(p => p.FirstLanguage, f => f.Random.Word())
                .RuleFor(p => p.SexualOrientation, f => f.Random.Word())
                .RuleFor(p => p.ContextFlag, f => f.Random.String2(1))
                .RuleFor(p => p.EmailAddress, f => f.Internet.Email())
                .RuleFor(p => p.Ethnicity, f => f.Random.Word())
                .RuleFor(p => p.DateOfBirth, f => f.Date.Past())
                .RuleFor(p => p.DateOfDeath, f => f.Date.Past())
                .RuleFor(p => p.ReviewDate, f => f.Date.Past())
                .RuleFor(p => p.FirstName, f => f.Person.FirstName)
                .RuleFor(p => p.LastName, f => f.Person.LastName)
                .RuleFor(p => p.Title, f => f.Random.String2(2))
                .RuleFor(p => p.Gender, f => f.Random.String2(1))
                .RuleFor(p => p.NhsNumber, f => f.Random.Number())
                .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.Word())
                .RuleFor(p => p.Religion, f => f.Random.Word())
                .RuleFor(p => p.Restricted, f => f.Random.String2(1))
                .RuleFor(p => p.CreatedBy, f => f.Internet.Email())
                .RuleFor(p => p.Address, address)
                .RuleFor(p => p.PhoneNumbers, phoneNumbers)
                .RuleFor(p => p.OtherNames, otherNames);
        }

        private static PatchPersonRequest GetValidPatchPersonRequest(long personId)
        {
            AddressDomain address = new Faker<AddressDomain>()
                .RuleFor(a => a.Address, f => f.Address.StreetAddress())
                .RuleFor(a => a.Postcode, f => f.Address.ZipCode())
                .RuleFor(a => a.Uprn, f => f.Random.Int());

            const string phoneNumberOne = "07755555555";
            const string phoneNumberTypeOne = "Mobile";

            const string phoneNumberTwo = "07977777777";
            const string phoneNumberTypeTwo = "Fax";

            var phoneNumbers = new List<PhoneNumber>()
            {
                new PhoneNumber() {Number = phoneNumberOne, Type = phoneNumberTypeOne},
                new PhoneNumber() {Number = phoneNumberTwo, Type = phoneNumberTypeTwo}
            };

            var otherNames = new List<OtherName>()
            {
                new Faker<OtherName>()
                    .RuleFor(o => o.FirstName, f => f.Person.FirstName)
                    .RuleFor(o => o.LastName, f => f.Person.LastName),
                new Faker<OtherName>()
                    .RuleFor(o => o.FirstName, f => f.Person.FirstName)
                    .RuleFor(o => o.LastName, f => f.Person.LastName)
            };

            var lastUpdated = new List<LastUpdatedDomain>()
            {
                new Faker<LastUpdatedDomain>()
                    .RuleFor(o => o.Type, f => f.Random.Word())
                    .RuleFor(o => o.UpdatedAt, f => f.Date.Recent()),
            };

            var keyContacts = new List<KeyContact>()
            {
                new Faker<KeyContact>()
                    .RuleFor(o => o.Name, f => f.Random.String2(3))
                    .RuleFor(o => o.Email, f => f.Internet.Email())
            };

            var techUse = new List<string>() { "Tech One", "Tech Two" };

            var disabilities = new List<string>() { "Disability One", "Disability Two" };

            var emailAddress = new List<EmailAddress>()
            {
                new Faker<EmailAddress>()
                    .RuleFor(o => o.Type, f => f.Random.Word())
                    .RuleFor(o => o.Email, f => f.Internet.Email())
            };

            GpDetailsDomain gpDetailsDomain = new Faker<GpDetailsDomain>()
                .RuleFor(a => a.Address, f => f.Address.StreetAddress())
                .RuleFor(a => a.Postcode, f => f.Address.ZipCode())
                .RuleFor(a => a.Email, f => f.Internet.Email())
                .RuleFor(a => a.PhoneNumber, f => f.Phone.PhoneNumber());

            return new Faker<PatchPersonRequest>()
                .RuleFor(p => p.Id, personId)
                .RuleFor(p => p.Pronoun, f => f.Random.String2(3))
                .RuleFor(p => p.GenderAssignedAtBirth, f => f.Random.Bool())
                .RuleFor(p => p.FirstLanguage, f => f.Random.Word())
                .RuleFor(p => p.ReviewDate, f => f.Date.Past())
                .RuleFor(p => p.PreferredLanguage, f => f.Random.Word())
                .RuleFor(p => p.FluentInEnglish, f => f.Random.Bool())
                .RuleFor(p => p.DifficultyMakingDecisions, f => f.Random.Bool())
                .RuleFor(p => p.InterpreterNeeded, f => f.Random.Bool())
                .RuleFor(p => p.CommunicationDifficulties, f => f.Random.Bool())
                .RuleFor(p => p.CommunicationDifficultiesDetails, f => f.Random.Word())
                .RuleFor(p => p.Employment, f => f.Random.Word())
                .RuleFor(p => p.ImmigrationStatus, f => f.Random.Word())
                .RuleFor(p => p.SexualOrientation, f => f.Random.Word())
                .RuleFor(p => p.ContextFlag, f => f.Random.String2(1))
                .RuleFor(p => p.EmailAddress, f => f.Internet.Email())
                .RuleFor(p => p.Ethnicity, f => f.Random.String2(20))
                .RuleFor(p => p.CreatedBy, f => f.Internet.Email())
                .RuleFor(p => p.DateOfBirth, f => f.Date.Past())
                .RuleFor(p => p.DateOfDeath, f => f.Date.Recent())
                .RuleFor(p => p.FirstName, f => f.Random.Word())
                .RuleFor(p => p.LastName, f => f.Random.Word())
                .RuleFor(p => p.Title, f => f.Random.String2(1))
                .RuleFor(p => p.Gender, f => f.Random.String2(1))
                .RuleFor(p => p.Nationality, f => f.Random.Word())
                .RuleFor(p => p.Title, f => f.Random.String2(5))
                .RuleFor(p => p.NhsNumber, f => f.Random.Number())
                .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.Word())
                .RuleFor(p => p.Religion, f => f.Random.Word())
                .RuleFor(p => p.AllocatedTeam, f => f.Random.Word())
                .RuleFor(p => p.Restricted, f => f.Random.String2(1))
                .RuleFor(p => p.Address, address)
                .RuleFor(p => p.PhoneNumbers, phoneNumbers)
                .RuleFor(p => p.OtherNames, otherNames)
                .RuleFor(p => p.LastUpdated, lastUpdated)
                .RuleFor(p => p.KeyContacts, keyContacts)
                .RuleFor(p => p.GpDetails, gpDetailsDomain)
                .RuleFor(p => p.TechUse, techUse)
                .RuleFor(p => p.Disabilities, disabilities)
                .RuleFor(p => p.Emails, emailAddress)
                .RuleFor(p => p.OtherNames, otherNames);
        }

        private WorkerTeam SaveWorkerTeamToDatabase(WorkerTeam workerTeam)
        {
            DatabaseContext.WorkerTeams.Add(workerTeam);
            DatabaseContext.SaveChanges();
            return workerTeam;
        }
    }
}
