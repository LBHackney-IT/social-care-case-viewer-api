using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using dbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using dbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
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
        public void CanMapWorkerFromInfrastructureToDomain()
        {
            var email = _faker.Internet.Email();
            var firstName = _faker.Name.ToString();
            var lastName = _faker.Name.ToString();
            var id = _faker.Random.Number();
            var role = _faker.Random.Word();
            int? teamId = null;

            var dbWorker = new dbWorker()
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                Role = role,
                TeamId = teamId
            };

            var expectedResponse = new Worker()
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id
            };

            dbWorker.ToDomain().Should().BeEquivalentTo(expectedResponse);
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
            var allocatedBy = _faker.Internet.Email(); //email
            var workerId = _faker.Random.Number();
            var dt = DateTime.Now;
            var caseStatus = "Open";

            var allocationRequest = new CreateAllocationRequest()
            {
                MosaicId = personId,
                AllocatedBy = allocatedBy,
                AllocatedWorkerId = workerId
            };

            var expectedResponse = new AllocationSet()
            {
                MosaicId = personId,
                WorkerId = workerId,
                AllocationStartDate = dt,
                CaseStatus = caseStatus
            };

            allocationRequest.ToEntity(workerId, dt, caseStatus).Should().BeEquivalentTo(expectedResponse);
        }
    }
}
