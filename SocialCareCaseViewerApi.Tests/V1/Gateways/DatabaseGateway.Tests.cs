using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
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
        private Mock<IProcessDataGateway> _mockProcessDataGateway;

        [SetUp]
        public void Setup()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _classUnderTest = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object);
        }

        [Test]
        public void CreatingAnAllocationShouldInsertIntoTheDatabase()
        {
            var request = _fixture.Build<CreateAllocationRequest>().Create();

            _classUnderTest.CreateAllocation(request);

            var query = DatabaseContext.Allocations;

            query.Count().Should().Be(1);

            var insertedRecord = query.First();
            insertedRecord.MosaicId.Should().NotBeNullOrEmpty();
            insertedRecord.MosaicId.Should().BeEquivalentTo(request.MosaicId.ToString());
            insertedRecord.WorkerEmail.Should().BeEquivalentTo(request.WorkerEmail);
            insertedRecord.AllocatedWorkerTeam.Should().BeEquivalentTo(request.AllocatedWorkerTeam);
        }
    }
}
