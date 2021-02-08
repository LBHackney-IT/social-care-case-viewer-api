using System;
using System.Collections.Generic;
using System.Dynamic;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using dbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using dbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Domain.Team;
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    [TestFixture]
    public class EntityFactoryTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        [Test]
        public void CanMapWorkerFromInfrastructureToDomainWithoutTeamDetails()
        {
            var email = _faker.Internet.Email();
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var id = 1;
            var role = _faker.Random.Word();
            int allocationCount = 2;

            var dbWorker = new dbWorker()
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                Role = role,
                Allocations = new List<AllocationSet>() {
                    new AllocationSet() { Id = 1, PersonId = 2 },
                    new AllocationSet() { Id = 2, PersonId = 3 }
                }
            };

            var expectedResponse = new Worker()
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                AllocationCount = allocationCount,
                Email = email,
                Role = role,
                Teams = null
            };

            dbWorker.ToDomain(false).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapWorkerFromInfrastructureToDomainWithTeamDetails()
        {
            var email = _faker.Internet.Email();
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var id = 1;
            var role = _faker.Random.Word();
            int allocationCount = 2;

            var dbWorker = new dbWorker()
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                Role = role,
                Allocations = new List<AllocationSet>() {
                    new AllocationSet() { Id = 1, PersonId = 2 },
                    new AllocationSet() { Id = 2, PersonId = 3 }
                },
                WorkerTeams = new List<WorkerTeam>()
                {
                    new WorkerTeam() { Id = 1 , Team = new dbTeam() { Id = 1, Name = "Team 1", Context = "C" } },
                    new WorkerTeam() { Id = 2 , Team = new dbTeam() { Id = 2, Name = "Team 2", Context = "C" } },
                }
            };

            var expectedResponse = new Worker()
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                AllocationCount = allocationCount,
                Email = email,
                Role = role,
                Teams = new List<Team>()
                {
                    new Team() { Id = 1, Name = "Team 1"},
                    new Team() { Id = 2, Name = "Team 2"}
                }
            };

            dbWorker.ToDomain(true).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapTeamFromInfrastrcutureToDomain()
        {
            var id = _faker.Random.Number();
            var name = _faker.Name.ToString();
            var context = "a";

            var dbTeam = new dbTeam()
            {
                Id = id,
                Name = name,
                Context = context
            };

            var exptectedResponse = new Team()
            {
                Id = id,
                Name = name
            };

            dbTeam.ToDomain().Should().BeEquivalentTo(exptectedResponse);
        }

        [Test]
        public void CanMapCreateAllocationRequestDomainObjectToDatabaseEntity()
        {
            var personId = _faker.Random.Long();
            var createdBy = _faker.Internet.Email(); 
            var workerId = _faker.Random.Number();
            var dt = DateTime.Now;
            var caseStatus = "Open";

            var allocationRequest = new CreateAllocationRequest()
            {
                MosaicId = personId,
                CreatedBy = createdBy,
                AllocatedWorkerId = workerId
            };

            var expectedResponse = new AllocationSet()
            {
                PersonId = personId,
                WorkerId = workerId,
                AllocationStartDate = dt,
                CaseStatus = caseStatus,
                CreatedBy = createdBy
            };

            allocationRequest.ToEntity(workerId, dt, caseStatus).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapOtherNameFromDomainToInfrastructure()
        {
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var createdBy = _faker.Internet.Email();

            var domainOtherName = new OtherName()
            {
                FirstName = firstName,
                LastName = lastName
            };

            long personId = 123;

            var infrastructureOtherName = new PersonOtherName()
            {
                FirstName = firstName,
                LastName = lastName,
                PersonId = personId,
                CreatedBy = createdBy
            };

            domainOtherName.ToEntity(personId, createdBy).Should().BeEquivalentTo(infrastructureOtherName);
        }

        [Test]
        public void CanMapPhoneNumberFromDomainToInfrastructure()
        {
            string phoneNumber = "12345678";
            string phoneNumberType = "Mobile";
            long personId = 123;
            string createdBy = _faker.Internet.Email();

            var domainNumber = new PhoneNumber()
            {
                Number = phoneNumber,
                Type = phoneNumberType
            };

            var infrastructurePhoneNumber = new dbPhoneNumber()
            {
                Number = phoneNumber.ToString(),
                PersonId = personId,
                Type = phoneNumberType,
                CreatedBy = createdBy
            };

            domainNumber.ToEntity(personId, createdBy).Should().BeEquivalentTo(infrastructurePhoneNumber);
        }
    }
}
