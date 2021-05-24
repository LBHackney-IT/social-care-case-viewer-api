using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class GetVisitByVisitIdUseCaseTests
    {
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformApiGateway;
        private GetVisitByVisitIdUseCase _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _mockSocialCarePlatformApiGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _classUnderTest = new GetVisitByVisitIdUseCase(_mockSocialCarePlatformApiGateway.Object);
        }

        [Test]
        public void GetVisitByVisitIdReturnsNullWhenNoVisitWithIdExists()
        {
            var response = _classUnderTest.Execute(0L);

            response.Should().BeNull();
        }

        [Test]
        public void GetVisitByVisitIdReturnsVisitWhenVisitWithIdExists()
        {
            var visit = TestHelpers.CreateVisit();
            _mockSocialCarePlatformApiGateway.Setup(x => x.GetVisitByVisitId(visit.VisitId)).Returns(visit);

            var response = _classUnderTest.Execute(visit.VisitId);

            response.Should().BeEquivalentTo(visit);
        }
    }
}
