using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class GetVisitByVisitIdUseCaseTests
    {
        private Mock<IHistoricalDataGateway> _historicalDataGateway = null!;
        private GetVisitByVisitIdUseCase _classUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            _historicalDataGateway = new Mock<IHistoricalDataGateway>();
            _classUnderTest = new GetVisitByVisitIdUseCase(_historicalDataGateway.Object);
        }

        [Test]
        public void ExecuteReturnsNullWhenNoVisitWithIdExists()
        {
            var response = _classUnderTest.Execute(0L);

            response.Should().BeNull();
        }

        [Test]
        public void ExecuteReturnsVisitWhenVisitWithIdExists()
        {
            var visit = TestHelpers.CreateVisit();
            _historicalDataGateway.Setup(x => x.GetVisitById(visit.VisitId)).Returns(visit);

            var response = _classUnderTest.Execute(visit.VisitId);

            response.Should().BeEquivalentTo(visit);
        }
    }
}
