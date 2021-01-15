using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DatabaseGatewayTests : DatabaseTests
    {
        private DatabaseGateway _classUnderTest;
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Faker _faker;

        [SetUp]
        public void Setup()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _classUnderTest = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object);
            _faker = new Faker();
        }

        [Test]
        public void CreatingAnAllocationShouldInsertIntoTheDatabase()
        {
            int workerId = 555666;
            long mosaicId = 5000555;
            string allocatedByEmail = _faker.Internet.Email();
            string workerEmail = _faker.Internet.Email();
            int teamId = 1000;
            string personName = $"{_faker.Name.FirstName()} {_faker.Name.LastName()}";

            var request = new CreateAllocationRequest()
            {
                AllocatedWorkerId = workerId,
                MosaicId = mosaicId,
                AllocatedBy = allocatedByEmail
            };

            //add test worker and team
            Worker worker = new Worker()
            {
                Email = workerEmail,
                Id = workerId,
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                TeamId = teamId
            };

            Worker allocatedByWorker = new Worker()
            {
                Email = allocatedByEmail,
                Id = _faker.Random.Number(),
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                TeamId = teamId
            };

            Team team = new Team()
            {
                Context = "A",
                Id = teamId,
                Name = "Test team"
            };

            Person person = new Person()
            {
                Id = mosaicId,
                FullName = personName
            };

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(allocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);

            DatabaseContext.SaveChanges();

            //TODO: add process data gw tests
            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>())).Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            _classUnderTest.CreateAllocation(request);

            var query = DatabaseContext.Allocations;

            //TODO: set this up so that local testing is easy and always run against clean datasets
            var insertedRecord = query.FirstOrDefault(x => x.MosaicId == mosaicId.ToString());

            Assert.IsNotNull(insertedRecord);
            insertedRecord.MosaicId.Should().NotBeNullOrEmpty();
            insertedRecord.MosaicId.Should().BeEquivalentTo(request.MosaicId.ToString());
            insertedRecord.WorkerEmail.Should().BeEquivalentTo(workerEmail);
        }
    }
}
