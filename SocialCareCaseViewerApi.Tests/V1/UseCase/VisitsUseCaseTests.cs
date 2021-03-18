using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class VisitsUseCaseTests
    {
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformAPIGateway;
        private VisitsUseCase _visitsUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _visitsUseCase = new VisitsUseCase(_mockSocialCarePlatformAPIGateway.Object);
        }

        [Test]
        public void GetVisitsCallsSocialCarePlatformAPIWhenPersonIdIsUsed()
        {
            var request = new ListVisitsRequest() { Id = "1" };

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetVisitsByPersonId(It.IsAny<string>())).Returns(new ListVisitsResponse());

            _visitsUseCase.ExecuteGetByPersonId(request.Id);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetVisitsByPersonId(It.IsAny<string>()));
        }

        [Test]
        public void GetVisitsCallsSocialCarePlatformAPIWithParameterWhenPersonIdIsUsed()
        {
            var request = new ListCaseNotesRequest() { Id = "1" };

            _mockSocialCarePlatformAPIGateway.Setup(x => x.GetVisitsByPersonId(request.Id)).Returns(new ListVisitsResponse());

            _visitsUseCase.ExecuteGetByPersonId(request.Id);

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetVisitsByPersonId(request.Id));
        }
    }
}
