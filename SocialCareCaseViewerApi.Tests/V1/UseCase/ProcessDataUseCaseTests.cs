using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class ProcessDataUseCaseTests
    {
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private CaseRecordsUseCase _classUnderTest;
        private Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _classUnderTest = new CaseRecordsUseCase(_mockProcessDataGateway.Object, _mockDatabaseGateway.Object);
        }

        [Test]
        public void ExecuteReturnsCareCaseDataWhenProvidedRecordId()
        {
            var stubbedCaseData = _fixture.Create<CareCaseData>();

            _mockProcessDataGateway.Setup(x => x.GetCaseById(It.IsAny<string>()))
                .Returns(stubbedCaseData);

            var response = _classUnderTest.Execute("test record id");

            response.Should().BeEquivalentTo(stubbedCaseData);
        }
    }
}
