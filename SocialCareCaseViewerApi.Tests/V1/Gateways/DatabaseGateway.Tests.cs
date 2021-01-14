using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DatabaseGatewayTests : DatabaseTests
    {
        private DatabaseGateway _classUnderTest;
        private Fixture _fixture = new Fixture();

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new DatabaseGateway(DatabaseContext);
        }

        [Test]
        public void CreatingAnAllocationShouldInsertIntoTheDatabase()
        {
            var request = _fixture.Build<CreateAllocationRequest>().Create();

            _classUnderTest.CreateAllocation(request);

            var query = DatabaseContext.Allocations;

            query.Count().Should().Be(1);

            var insertedRecord = query.First();
            insertedRecord.Id.Should().NotBeNullOrEmpty();
            insertedRecord.Id.Should().BeEquivalentTo(request.MosaicId.ToString());
            insertedRecord.WorkerEmail.Should().BeEquivalentTo(request.WorkerEmail);
            insertedRecord.AllocatedWorkerTeam.Should().BeEquivalentTo(request.AllocatedWorkerTeam);
        }
    }
}
