using System;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ProcessDataGatewayTests : MongoDbTests
    {
        private ProcessDataGateway _classUnderTest;
        private Mock<ISocialCarePlatformAPIGateway> _mockSocialCarePlatformAPIGateway;
        private Mock<ISccvDbContext> _mockSccvDbContext;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockSccvDbContext = new Mock<ISccvDbContext>();
            _mockSocialCarePlatformAPIGateway = new Mock<ISocialCarePlatformAPIGateway>();
            _mockSccvDbContext.Setup(x => x.getCollection()).Returns(collection);
            _classUnderTest = new ProcessDataGateway(_mockSccvDbContext.Object, _mockSocialCarePlatformAPIGateway.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void ProcessDataCanCallSocialCarePlatformGatewayForAListOfHistoricCaseNotes()
        {
            var request = _fixture.Create<ListCasesRequest>();
            request.MosaicId = "1";

            _classUnderTest.GetProcessData(request, "2");

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetCaseNotesByPersonId(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void WhenFeatureFlagIsTrueProcessDataCanCallSocialCarePlatformGatewayForListOfVisits()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA", "true");
            var request = _fixture.Create<ListCasesRequest>();
            request.MosaicId = "1";

            _classUnderTest.GetProcessData(request, "2");

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetVisitsByPersonId(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void WhenFeatureFlagIsNotTrueProcessDataShouldNotCallSocialCarePlatformGatewayForListOfVisits()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA", "false");
            var request = _fixture.Create<ListCasesRequest>();
            request.MosaicId = "1";

            _classUnderTest.GetProcessData(request, "2");

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetVisitsByPersonId(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void WhenFeatureFlagIsNullProcessDataShouldNotCallSocialCarePlatformGatewayForListOfVisits()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_SHOW_HISTORIC_DATA", null);
            var request = _fixture.Create<ListCasesRequest>();
            request.MosaicId = "1";

            _classUnderTest.GetProcessData(request, "2");

            _mockSocialCarePlatformAPIGateway.Verify(x => x.GetVisitsByPersonId(It.IsAny<string>()), Times.Never());
        }
    }
}
