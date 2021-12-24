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
        private Mock<IHistoricalSocialCareGateway> _historicalSocialCareGateway = null!;
        private GetVisitByVisitIdUseCase _classUnderTest = null!;

        [SetUp]
        public void SetUp()
        {
            _historicalSocialCareGateway = new Mock<IHistoricalSocialCareGateway>();
            _classUnderTest = new GetVisitByVisitIdUseCase(_historicalSocialCareGateway.Object);
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
            _historicalSocialCareGateway.Setup(x => x.GetVisitInformationByVisitId(visit.VisitId)).Returns(visit);

            var response = _classUnderTest.Execute(visit.VisitId);

            response.Should().BeEquivalentTo(visit);
        }
    }
}
